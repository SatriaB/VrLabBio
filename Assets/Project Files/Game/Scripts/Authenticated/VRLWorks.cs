using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FatahDev
{
    public enum WorkStepGroupId
    {
        Microscope        = 0, // Mikroskop
        Caliper           = 1, // Jangka Sorong
        Micrometer        = 2, // Micrometer
        AnalyticalBalance = 3  // Neraca Analitik
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
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + auth.Token);
                req.timeout = _timeoutSeconds;

                yield return req.SendWebRequest();

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

        // ===== Coroutine runner (otomatis dibuat sekali) =====
        private class Runner : MonoBehaviour { }
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
