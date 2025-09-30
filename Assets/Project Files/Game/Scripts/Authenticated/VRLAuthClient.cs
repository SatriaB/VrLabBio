using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FatahDev
{
    [Serializable] public class LoginRequest { public string email; public string password; }

    [Serializable] public class VRLUser {
        public int id;
        public string name;
        public string email;
        public string email_verified_at;
        public string created_at;
        public string updated_at;
    }

    [Serializable] public class LoginResponse {
        public bool status;
        public string message;
        public VRLUser data;
        public string token;
    }

    public class VRLAuthClient : MonoBehaviour
    {
        [Header("API")]
        [Tooltip("Base URL tanpa slash di akhir")]
        public string baseUrl = "https://vrlabolatory.bio";
        [Tooltip("Path endpoint login dari dokumentasimu")]
        public string loginPath = "api/login"; // ganti ke /api/login jika perlu
        public float timeoutSeconds = 15f;

        public IEnumerator Login(string email, string password,
                                 Action<LoginResponse> onSuccess,
                                 Action<string> onError)
        {
            var payload = new LoginRequest { email = email, password = password };
            var json = JsonUtility.ToJson(payload);
            var url  = $"{baseUrl.TrimEnd('/')}/{loginPath.TrimStart('/')}";

            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler   = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.timeout = Mathf.RoundToInt(timeoutSeconds);

            yield return req.SendWebRequest();
                
            bool hasError = req.result != UnityWebRequest.Result.Success;

            if (hasError)
            {
                onError?.Invoke($"HTTP {(int)req.responseCode}: {req.error}");
                yield break;
            }

            var text = req.downloadHandler.text;
            LoginResponse resp = null;
            try { resp = JsonUtility.FromJson<LoginResponse>(text); }
            catch (Exception ex) { onError?.Invoke($"Parse error: {ex.Message}\n{text}"); yield break; }

            if (resp == null || !resp.status || string.IsNullOrEmpty(resp.token))
            {
                onError?.Invoke(resp != null ? (resp.message ?? "Login gagal") : "Response null");
                yield break;
            }

            onSuccess?.Invoke(resp);
        }
    }
}
