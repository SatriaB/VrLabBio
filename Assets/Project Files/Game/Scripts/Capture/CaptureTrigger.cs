using UnityEngine;
using UnityEngine.InputSystem;

namespace FatahDev
{
    /// <summary>
    /// Taruh di GameObject global (mis. @Input). 
    /// Isi captureAction dengan XRI RightHand → Secondary Button (B).
    /// </summary>
    public class CaptureTrigger : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference captureAction;

        [Header("Force Provider (opsional)")]
        [SerializeField] private GenericCaptureProvider forcedProvider;

        private void OnEnable()
        {
            if (captureAction != null)
            {
                captureAction.action.performed += OnCapture;
                captureAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (captureAction != null)
            {
                captureAction.action.performed -= OnCapture;
                captureAction.action.Disable();
            }
        }

        private void OnCapture(InputAction.CallbackContext ctx)
        {
            var provider = forcedProvider != null ? forcedProvider : CaptureRouter.ActiveProvider;
            if (provider == null)
            {
                Debug.LogWarning("[Capture] No active provider.");
                return;
            }

            var cam = provider.SourceCamera;
            var meta = provider.SnapshotMetadata();
            var opt  = provider.BuildOptions();

            CaptureService.Instance.RequestCapture(cam, provider.ModuleName, meta, opt);
        }
    }
}