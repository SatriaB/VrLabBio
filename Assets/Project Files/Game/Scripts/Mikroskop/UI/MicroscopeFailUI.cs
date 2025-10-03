using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FatahDev
{
    public class MicroscopeFailUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private CanvasGroup overlay;          // Panel gabungan (satu-satunya UI)
        [SerializeField] private TextMeshProUGUI messageText;  // Teks info fail
        [SerializeField] private Button restartButton;         // Tombol restart

        [Header("Behaviour")]
        [SerializeField] private bool pauseOnFail = true;      // Auto-pause saat gagal

        MicroscopeDropGuard guard; // dicari otomatis

        void Awake()
        {
            // Cari guard (boleh satu scene)
            guard = FindObjectOfType<MicroscopeDropGuard>(includeInactive: true);

            // Inisialisasi UI off
            if (overlay)
            {
                overlay.alpha = 0f;
                overlay.blocksRaycasts = false;
                overlay.interactable = false;
            }

            if (restartButton) restartButton.onClick.AddListener(RestartNow);
        }

        void OnEnable()
        {
            if (guard != null)
                guard.OnFail.AddListener(ShowFail); // UI SATU PAKET
        }

        void OnDisable()
        {
            if (guard != null)
                guard.OnFail.RemoveListener(ShowFail);
        }

        // Dipanggil otomatis saat fail
        public void ShowFail(string reason)
        {
            if (pauseOnFail) Time.timeScale = 0f;

            if (messageText) messageText.text = string.IsNullOrEmpty(reason)
                ? "Percobaan gagal."
                : reason;

            if (overlay)
            {
                overlay.alpha = 1f;
                overlay.blocksRaycasts = true;
                overlay.interactable = true;
            }
        }

        public void RestartNow()
        {
            // pastikan unpause sebelum reload
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
