using System; // <-- penting buat EventHandler
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI; // NonNativeKeyboard

namespace FatahDev
{
    public class VRLLoginPanel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PanelRouter panelRouter;
        [SerializeField] private VRLAuthClient authClient;

        [Header("UI")]
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private Button loginButton;
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("On-screen Keyboard")]
        [SerializeField] private NonNativeKeyboard keyboard;

        private TMP_InputField _currentField;

        private void OnEnable()
        {
            if (loginButton)
            {
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.AddListener(OnClickLogin);
            }

            if (keyboard == null) keyboard = FindObjectOfType<NonNativeKeyboard>(true);

            if (emailField)    { emailField.onSelect.RemoveAllListeners();    emailField.onSelect.AddListener(_ => OpenKeyboardFor(emailField)); }
            if (passwordField) { passwordField.onSelect.RemoveAllListeners(); passwordField.onSelect.AddListener(_ => OpenKeyboardFor(passwordField)); }

            if (keyboard != null)
            {
                keyboard.OnTextUpdated    += OnKbTextUpdated;      // <-- biasanya string
            }

            SetLoading(false);
            if (messageText) messageText.text = "";
        }

        private void OnDisable()
        {
            if (keyboard != null)
            {
                keyboard.OnTextUpdated    -= OnKbTextUpdated;
            }
        }

        private void OpenKeyboardFor(TMP_InputField field)
        {
            if (keyboard == null || field == null) return;
            _currentField = field;
            keyboard.gameObject.SetActive(true);
            keyboard.PresentKeyboard(field.text ?? string.Empty);
        }

        // --- Handler ---

        // VARIAN UMUM: OnTextUpdated(string text)
        private void OnKbTextUpdated(string text)
        {
            if (_currentField != null) _currentField.text = text;
        }

        private void CloseKeyboard()
        {
            if (keyboard == null) return;
            //keyboard.Close();
            keyboard.gameObject.SetActive(false);
        }

        // --- Login flow tetap sama ---
        private void SetLoading(bool isLoading)
        {
            if (loginButton) loginButton.interactable = !isLoading;
            if (loadingSpinner) loadingSpinner.SetActive(isLoading);
        }

        private void OnClickLogin()
        {
            var email = emailField ? emailField.text.Trim() : "";
            var pass  = passwordField ? passwordField.text : "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                if (messageText) messageText.text = "Email dan password wajib diisi.";
                return;
            }

            CloseKeyboard();
            SetLoading(true);
            if (messageText) messageText.text = "";
            StartCoroutine(DoLogin(email, pass));
        }

        private System.Collections.IEnumerator DoLogin(string email, string pass)
        {
            yield return authClient.Login(email, pass,
                onSuccess: resp =>
                {
                    VRLAuthState.Instance.ApplyLogin(resp);
                    if (messageText) messageText.text = "Login sukses";
                    panelRouter.ShowMainMenu();
                },
                onError: err =>
                {
                    if (messageText) messageText.text = err;
                    SetLoading(false);
                });
        }
    }
}
