using UnityEngine;

namespace FatahDev
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Routing")]
        [SerializeField] private PanelRouter panelRouter;

        [Header("Optional groups (aktif/nonaktif saat Play)")]
        [SerializeField] private GameObject[] menuOnlyObjects;
        [SerializeField] private GameObject[] gameOnlyObjects;

        [SerializeField] private CharacterController characterController;

        [Header("Login Gate")]
        [SerializeField] private bool requireLogin = true;

        private void Start()
        {
            // Default: kunci movement di awal
            if (characterController != null) characterController.enabled = false;

            // Jika butuh login dan belum ada token → tampilkan panel Login
            if (requireLogin && string.IsNullOrEmpty(VRLAuthState.Instance?.Token))
            {
                if (panelRouter != null) panelRouter.ShowLogin();
                return;
            }

            Debug.LogWarning("VRLAuthState.Instance is null " + VRLAuthState.Instance?.Token);
            // Kalau sudah login / tidak butuh login → seperti flow lama
            if (panelRouter != null) panelRouter.ShowMainMenu();
            SetGroupActive(menuOnlyObjects, true);
            SetGroupActive(gameOnlyObjects, false);
        }

        public void OnClickPlay()
        {
            if (characterController != null) characterController.enabled = true;
            SetGroupActive(menuOnlyObjects, false);
            SetGroupActive(gameOnlyObjects, true);
            panelRouter.ShowHud();
        }

        public void OnOpenSettings() => panelRouter.ShowSettings();
        public void OnCloseSettings() => panelRouter.ShowMainMenu();
        public void OnOpenTutorial() => panelRouter.ShowTutorial();
        public void OnCloseTutorial() => panelRouter.ShowMainMenu();

        public void OnExitToMenuFromHud()
        {
            SetGroupActive(menuOnlyObjects, true);
            SetGroupActive(gameOnlyObjects, false);
            panelRouter.ShowMainMenu();
        }

        public void OnEndSessionFromMenu()
        {
            SetGroupActive(menuOnlyObjects, true);
            SetGroupActive(gameOnlyObjects, false);
            panelRouter.ShowMainMenu();
        }
        
        public void OnClickLogout()
        {
            VRLAuthState.Instance.SignOut();
            panelRouter.ShowLogin();
        }

        private static void SetGroupActive(GameObject[] arr, bool active)
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++) if (arr[i]) arr[i].SetActive(active);
        }
    }
}
