using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ClampController : MonoBehaviour
    {
        [Header("Target Clamp (yang bergerak)")]
        [SerializeField] private Transform clampTransform;
        [SerializeField] private Vector3 localAxis = Vector3.right;
        [SerializeField] private bool axisIsLocal = true;

        [Header("Batas Rotasi (derajat)")]
        [SerializeField] private float minDegrees = 0f;
        [SerializeField] private float maxDegrees = 25f;

        [Header("Threshold Open/Close")]
        [Range(0f, 1f)] [SerializeField] private float openThreshold = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float closeThreshold = 0.2f;

        [Header("Behaviour")]
        [SerializeField] private bool autoReturn = true;   // balik otomatis ke Closed

        [Header("Slide Socket (opsional)")]
        [SerializeField] private XRSocketInteractor slideSocket;

        [Header("Events")]
        public UnityEvent OnClampOpened;
        public UnityEvent OnClampClosed;
        public UnityEvent<float> OnValueChanged;

        private XRSimpleInteractable interactable;
        private IXRSelectInteractor selecting;
        private Vector3 startDirOnPlane;
        private Vector3 prevDirOnPlane;
        private Vector3 axisWorld;

        private float currentAngleDeg = 0f;
        private Quaternion zeroRotWorld;

        private bool isOpened = false;

        void Reset()
        {
            clampTransform = transform;
        }

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (clampTransform == null) clampTransform = transform;

            zeroRotWorld = clampTransform.rotation;
            currentAngleDeg = Mathf.Clamp(currentAngleDeg, minDegrees, maxDegrees);
            ApplyClampRotation(currentAngleDeg);
        }

        void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            selecting = args.interactorObject;

            axisWorld = (axisIsLocal ? clampTransform.TransformDirection(localAxis) : localAxis).normalized;

            var center = clampTransform.position;
            var hand = GetInteractorAttach(selecting, args.interactableObject);
            startDirOnPlane = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;
            prevDirOnPlane = startDirOnPlane;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (selecting == args.interactorObject)
                selecting = null;

            // Auto return kalau diaktifkan
            if (autoReturn)
            {
                currentAngleDeg = minDegrees;
                ApplyClampRotation(currentAngleDeg);
                isOpened = false;
                OnClampClosed?.Invoke();
                OnValueChanged?.Invoke(0f);

                // Disable socket area jika clamp ditutup
                UpdateSocketState(false);
            }
        }

        void Update()
        {
            if (selecting == null) return;

            var center = clampTransform.position;
            var hand = GetInteractorAttach(selecting, interactable);
            var nowDir = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;

            float frameDelta = Vector3.SignedAngle(prevDirOnPlane, nowDir, axisWorld);
            prevDirOnPlane = nowDir;

            currentAngleDeg += frameDelta;
            currentAngleDeg = Mathf.Clamp(currentAngleDeg, minDegrees, maxDegrees);

            float t = Mathf.InverseLerp(minDegrees, maxDegrees, currentAngleDeg);
            OnValueChanged?.Invoke(t);

            if (!isOpened && t >= openThreshold)
            {
                isOpened = true;
                OnClampOpened?.Invoke();
                UpdateSocketState(true); // aktifkan socket kalau clamp terbuka
            }
            else if (isOpened && t <= closeThreshold)
            {
                isOpened = false;
                OnClampClosed?.Invoke();
                UpdateSocketState(false); // matikan socket kalau clamp tertutup
            }

            ApplyClampRotation(currentAngleDeg);
        }

        private void ApplyClampRotation(float angleDeg)
        {
            clampTransform.rotation = Quaternion.AngleAxis(angleDeg, axisWorld) * zeroRotWorld;
        }

        private void UpdateSocketState(bool canPlace)
        {
            if (slideSocket == null) return;

            Collider col = slideSocket.GetComponent<Collider>();
            if (col == null) return;

            // Jangan matikan collider kalau sudah ada slide di dalam
            if (!canPlace && slideSocket.hasSelection)
                return;

            col.enabled = canPlace;
        }

        private static Transform GetInteractorAttach(IXRSelectInteractor interactor, IXRInteractable forThis)
        {
            var t = interactor.GetAttachTransform(forThis);
            return t != null ? t : (interactor as Component).transform;
        }
    }
}
