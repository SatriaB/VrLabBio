using UnityEngine;

namespace FatahDev
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Routing")]
        [SerializeField] private PanelRouter panelRouter;

        [Header("Optional groups (aktif/nonaktif saat Play)")]
        [SerializeField] private GameObject[] menuOnlyObjects; // misal: backdrops/ornamen menu
        [SerializeField] private GameObject[] gameOnlyObjects; // misal: helper gameworld, marker, dll.
        
        [SerializeField] private CharacterController characterController;

        private void Start()
        {
            // Buka Main Menu saat start scene
            if (panelRouter != null) panelRouter.ShowMainMenu();
            SetGroupActive(menuOnlyObjects, true);
            SetGroupActive(gameOnlyObjects, false);
            
            if (characterController != null)  characterController.enabled = false;
        }

        // === Buttons ===
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

        private static void SetGroupActive(GameObject[] arr, bool active)
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++) if (arr[i]) arr[i].SetActive(active);
        }
    }
}
