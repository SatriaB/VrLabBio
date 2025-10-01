using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private XROrigin xrOrigin;

        [Header("Detect Area")] [SerializeField]
        private float maxDistanceRay = 1.2f;

        [SerializeField] private string lensTag = "Lens";

        [Header("Microscope View")] [SerializeField]
        private Camera mikroskopCamera;
        [SerializeField] private GenericCaptureProvider captureProvider;

        [SerializeField] private RawImage mikroskopImage;
        [SerializeField] private CanvasGroup mikroskopCanvas;

        [Header("Layers")] [SerializeField] private string lensLayerName = "LensView";

        [SerializeField] private bool microscopePowerOn = false; // default OFF

        private Camera mainCamera;
        private bool mikroskopEnabled;
        private int lensMask;

        private void Awake()
        {
            lensMask = LayerMask.GetMask(lensLayerName);
            if (lensMask == 0)
                Debug.LogWarning($"[Microscope] Layer '{lensLayerName}' belum dibuat/terisi.");
        }

        private void Start()
        {
            mainCamera = xrOrigin.Camera;

            // Set culling mask dua kamera
            if (mikroskopCamera && lensMask != 0)
            {
                mikroskopCamera.cullingMask = lensMask;
                //mikroskopCamera.stereoTargetEye = StereoTargetEyeMask.None; // penting!
            }

            if (mainCamera && lensMask != 0)
            {
                mainCamera.cullingMask &= ~lensMask;
            }

            if (mikroskopCamera && mikroskopCamera.targetTexture && mikroskopImage)
                mikroskopImage.texture = mikroskopCamera.targetTexture;

            SetMicroscope(false, instant: true);
        }

        private void Update()
        {
            if (!xrOrigin) return;

            var cam = xrOrigin.Camera.transform;
            bool inArea = Physics.Raycast(cam.position, cam.forward, out var hit, maxDistanceRay)
                          && hit.collider.CompareTag(lensTag);

            SetMicroscope(inArea);
        }

        private void SetMicroscope(bool on, bool instant = false)
        {
            if (mikroskopEnabled == on && !instant) return;
            mikroskopEnabled = on;

            if (mikroskopCamera)
            {
                mikroskopCamera.enabled = on;
                CaptureRouter.SetActiveProvider(captureProvider);
            }

            if (!mikroskopCanvas) return;

            bool showCanvas = on && microscopePowerOn;

            if (instant)
            {
                mikroskopCanvas.alpha = showCanvas ? 1f : 0f;
                mikroskopCanvas.blocksRaycasts = showCanvas;
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(Fade(mikroskopCanvas, showCanvas ? 1f : 0f, 0.12f));
            }
        }
        
        public void SetMicroscopePower(bool on)
        {
            microscopePowerOn = on;
            // Refresh instan: kamera tetap sesuai "ngintip", canvas menyesuaikan power
            SetMicroscope(mikroskopEnabled, instant: true);
        }
        
        public void ToggleMicroscopePower() => SetMicroscopePower(!microscopePowerOn);

        private System.Collections.IEnumerator Fade(CanvasGroup cg, float target, float dur)
        {
            float start = cg.alpha, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, target, t / dur);
                yield return null;
            }

            cg.alpha = target;
            cg.blocksRaycasts = target > 0.5f;
        }

        private void OnDrawGizmos()
        {
            if (!xrOrigin) return;
            var cam = xrOrigin.Camera.transform;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cam.position, cam.position + cam.forward * maxDistanceRay);
        }
    }
}