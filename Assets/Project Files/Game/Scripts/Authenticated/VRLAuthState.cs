using UnityEngine;

namespace FatahDev
{
    public class VRLAuthState : MonoBehaviour
    {
        public static VRLAuthState Instance { get; private set; }

        public string Token { get; private set; }
        public VRLUser CurrentUser { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ApplyLogin(LoginResponse resp)
        {
            Token = resp.token;
            CurrentUser = resp.data;
        }

        public void SignOut()
        {
            Token = null;
            CurrentUser = null;
        }
    }
}