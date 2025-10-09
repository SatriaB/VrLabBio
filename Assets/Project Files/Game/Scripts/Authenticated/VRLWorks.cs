using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FatahDev
{
    public enum WorkStepGroupId
    {
        Titration = 1,
        Titration2 =2,
        Microscope = 3, // Mikroskop
        Rulers = 4,
        Caliper = 5, // Jangka Sorong
        Micrometer = 6, // Micrometer
        AnalyticalBalance = 6 // Neraca Analitik
    }

    [Serializable]
    internal class CompleteStepRequest
    {
        public int work_step_id;
        public bool is_completed;
        public string result;
    }

    public static class VRLWorks
    {
        // ===== Config =====
        static string _baseUrl = "https://vrlabolatory.bio";
        static string _routeTemplate = "/api/works/{0}/complete-step";
        static int _timeoutSeconds = 15;

        static string _uploadRouteTemplate = "/api/works/{0}/upload-file";

        public static void Configure(string baseUrl = null, string routeTemplate = null, int? timeoutSeconds = null)
        {
            if (!string.IsNullOrEmpty(baseUrl)) _baseUrl = baseUrl;
            if (!string.IsNullOrEmpty(routeTemplate)) _routeTemplate = routeTemplate;
            if (timeoutSeconds.HasValue) _timeoutSeconds = Mathf.Max(1, timeoutSeconds.Value);
        }

        // ===== Public API (callback: success + raw body/error) =====
        public static void CompleteStep(WorkStepGroupId group, int workStepId, bool isCompleted, string result,
            Action<bool, string> onDone = null)
        {
            EnsureRunner();
            _runner.StartCoroutine(CompleteStepRoutine(group, workStepId, isCompleted, result, onDone));
        }

        // Convenience shortcuts per group (biar satu baris)
        public static void CompleteMicroscope(int stepId, string result, Action<bool, string> cb = null)
            => CompleteStep(WorkStepGroupId.Microscope, stepId, true, result, cb);

        public static void CompleteCaliper(int stepId, string result, Action<bool, string> cb = null)
            => CompleteStep(WorkStepGroupId.Caliper, stepId, true, result, cb);

        public static void CompleteMicrometer(int stepId, string result, Action<bool, string> cb = null)
            => CompleteStep(WorkStepGroupId.Micrometer, stepId, true, result, cb);

        public static void CompleteAnalyticalBalance(int stepId, string result, Action<bool, string> cb = null)
            => CompleteStep(WorkStepGroupId.AnalyticalBalance, stepId, true, result, cb);

        // ===== Internal =====
        private static IEnumerator CompleteStepRoutine(WorkStepGroupId group, int workStepId, bool isCompleted,
            string result, Action<bool, string> onDone)
        {
            // pastikan sudah login
            var auth = VRLAuthState.Instance;
            if (auth == null || string.IsNullOrEmpty(auth.Token))
            {
                onDone?.Invoke(false, "Not authenticated (token kosong).");
                yield break;
            }

            var path = string.Format(_routeTemplate, (int)group);
            var url = $"{_baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

            var body = new CompleteStepRequest
            {
                work_step_id = workStepId,
                is_completed = isCompleted,
                result = result ?? string.Empty
            };
            var json = JsonUtility.ToJson(body);

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                //UnityWebRequest req = new UnityWebRequest(url, "POST");
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + auth.Token);
                req.timeout = _timeoutSeconds;

                yield return req.SendWebRequest();
            Debug.Log("URL : " + url);
            Debug.Log("JSON : " + json);

#if UNITY_2020_2_OR_NEWER
            bool hasError = req.result != UnityWebRequest.Result.Success;
#else
                bool hasError = req.isNetworkError || req.isHttpError;
#endif
                if (hasError)
                {
                    onDone?.Invoke(false, $"HTTP {(int)req.responseCode}: {req.error}\n{req.downloadHandler?.text}");
                    yield break;
                }

                onDone?.Invoke(true, req.downloadHandler.text);
            }
        }

        public static void UploadFile(
            WorkStepGroupId group,
            int workStepId,
            byte[] fileBytes,
            string fileName,
            string mimeType = "image/png",
            Action<bool, string> onDone = null)
        {
            UploadFile(group, workStepId, fileBytes, fileName, mimeType, null, onDone);
        }

        public static void UploadTexturePNG(
            WorkStepGroupId group,
            int workStepId,
            Texture2D tex,
            string fileName = "capture.png",
            Action<bool, string> onDone = null)
        {
            if (!tex)
            {
                onDone?.Invoke(false, "Texture null.");
                return;
            }

            var png = tex.EncodeToPNG();
            UploadFile(group, workStepId, png, fileName, "image/png", onDone);
        }

        public static void UploadTextureJPG(
            WorkStepGroupId group,
            int workStepId,
            Texture2D tex,
            int jpgQuality = 90,
            string fileName = "capture.jpg",
            Action<bool, string> onDone = null)
        {
            if (!tex)
            {
                onDone?.Invoke(false, "Texture null.");
                return;
            }
#if UNITY_2020_1_OR_NEWER
            var jpg = tex.EncodeToJPG(jpgQuality);
#else
    var jpg = tex.EncodeToJPG();
#endif
            UploadFile(group, workStepId, jpg, fileName, "image/jpeg", onDone);
        }

        private static IEnumerator UploadFileRoutine(
            WorkStepGroupId group,
            int workStepId,
            byte[] fileBytes,
            string fileName,
            string mimeType,
            int? fieldId,
            Action<bool, string> onDone)
        {
            var auth = VRLAuthState.Instance;
            if (auth == null || string.IsNullOrEmpty(auth.Token))
            {
                onDone?.Invoke(false, "Not authenticated (token kosong).");
                yield break;
            }

            var path = string.Format(_uploadRouteTemplate, (int)group);
            var url = $"{_baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

            var form = new System.Collections.Generic.List<IMultipartFormSection>
            {
                new MultipartFormDataSection("work_step_id", workStepId.ToString())
            };
            if (fieldId.HasValue)
                form.Add(new MultipartFormDataSection("field_id", fieldId.Value.ToString()));

            form.Add(new MultipartFormFileSection("file", fileBytes, fileName, mimeType));

            using (var req = UnityWebRequest.Post(url, form))
            {
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + auth.Token);
                req.timeout = _timeoutSeconds;

                yield return req.SendWebRequest();
                Debug.Log("CompleteStep");

#if UNITY_2020_2_OR_NEWER
                bool hasError = req.result != UnityWebRequest.Result.Success;
#else
        bool hasError = req.isNetworkError || req.isHttpError;
#endif
                if (hasError)
                {
                    onDone?.Invoke(false, $"HTTP {(int)req.responseCode}: {req.error}\n{req.downloadHandler?.text}");
                    yield break;
                }

                onDone?.Invoke(true, req.downloadHandler.text);
            }
        }


// === Tambah di dalam class VRLWorks ===
        public static void UploadFile(
            WorkStepGroupId group,
            int workStepId,
            byte[] fileBytes,
            string fileName,
            string mimeType,
            int? fieldId,
            Action<bool, string> onDone = null)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                onDone?.Invoke(false, "fileBytes kosong.");
                return;
            }
            if (string.IsNullOrEmpty(fileName)) fileName = "capture.png";
            if (string.IsNullOrEmpty(mimeType)) mimeType = "application/octet-stream";

            EnsureRunner();
            _runner.StartCoroutine(UploadFileRoutine(group, workStepId, fileBytes, fileName, mimeType, fieldId, onDone));
        }

// Praktis: Texture PNG/JPG + fieldId opsional
        public static void UploadTexturePNG(
            WorkStepGroupId group, int workStepId, Texture2D tex,
            string fileName = "capture.png", int? fieldId = null,
            Action<bool, string> onDone = null)
        {
            if (!tex) { onDone?.Invoke(false, "Texture null."); return; }
            var png = tex.EncodeToPNG();
            UploadFile(group, workStepId, png, fileName, "image/png", fieldId, onDone);
        }

        public static void UploadTextureJPG(
            WorkStepGroupId group, int workStepId, Texture2D tex,
            int jpgQuality = 90, string fileName = "capture.jpg", int? fieldId = null,
            Action<bool, string> onDone = null)
        {
            if (!tex) { onDone?.Invoke(false, "Texture null."); return; }
#if UNITY_2020_1_OR_NEWER
            var jpg = tex.EncodeToJPG(jpgQuality);
#else
    var jpg = tex.EncodeToJPG();
#endif
            UploadFile(group, workStepId, jpg, fileName, "image/jpeg", fieldId, onDone);
        }

        // ===== Coroutine runner (otomatis dibuat sekali) =====
        private class Runner : MonoBehaviour
        {
        }

        private static Runner _runner;

        private static void EnsureRunner()
        {
            if (_runner != null) return;
            var go = new GameObject("[VRLWorks]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<Runner>();
        }
    }
}