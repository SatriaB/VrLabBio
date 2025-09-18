using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class KnobInteractable : MonoBehaviour
    {
        [Header("Target Knob")]
        [SerializeField] private Transform knobTransform;
        [SerializeField] private Vector3 localAxis = Vector3.forward;
        [SerializeField] private bool axisIsLocal = true;

        [Header("Batas (derajat)")]
        [SerializeField] private bool useLimits = true;
        [SerializeField] private float minDegrees = 0f;
        [SerializeField] private float maxDegrees = 90f;

        [Header("Snapping / Steps (opsional)")]
        [SerializeField] private bool useSnap = false;
        [SerializeField] private float snapStep = 10f;
        [SerializeField] private bool useSteps = false;
        [SerializeField] private int steps = 5;

        [Header("Output (0..1)")]
        [SerializeField] private bool invertOutput = false;
        public UnityEvent<float> OnValueChanged;

        [SerializeField] private Transform knob;
        [SerializeField] private InputActionReference triggerAction;

        private XRSimpleInteractable interactable;
        private IXRSelectInteractor selecting;
        private Vector3 startDirOnPlane;
        private Vector3 prevDirOnPlane;
        private Vector3 axisWorld;

        // ==== Tambahan untuk anti-wrap ====
        private float currentAngleDeg = 0f;     // sudut absolut yang diakumulasi (tidak wrap 180)
        private Quaternion zeroRotWorld;        // referensi 0° (rotasi awal di scene)

        void Reset()
        {
            knobTransform = transform;
        }

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (knobTransform == null) knobTransform = transform;

            // Set referensi 0° ke rotasi awal object di scene
            zeroRotWorld = knobTransform.rotation;

            // Optional: kalau mau posisi awal bukan 0°, isi currentAngleDeg via Inspector
            currentAngleDeg = Mathf.Clamp(currentAngleDeg, minDegrees, maxDegrees);
            ApplyKnobRotation(currentAngleDeg);
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

            axisWorld = (axisIsLocal ? knobTransform.TransformDirection(localAxis) : localAxis).normalized;

            var center = knobTransform.position;
            var hand = GetInteractorAttach(selecting, args.interactableObject);
            startDirOnPlane = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;

            // Mulai integrasi delta dari arah awal ini
            prevDirOnPlane = startDirOnPlane;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (selecting == args.interactorObject) selecting = null;
        }

        void Update()
        {
            if (selecting == null) return;

            var center = knobTransform.position;
            var hand = GetInteractorAttach(selecting, interactable);
            var nowDir = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;

            // Delta kecil per-frame (anti-wrap). Ini bisa lewat >180° total karena diakumulasikan.
            float frameDelta = Vector3.SignedAngle(prevDirOnPlane, nowDir, axisWorld);
            prevDirOnPlane = nowDir;

            // Akumulasi sudut absolut
            currentAngleDeg += frameDelta;

            // Batas (kalau diaktifkan)
            if (useLimits)
                currentAngleDeg = Mathf.Clamp(currentAngleDeg, minDegrees, maxDegrees);

            float outAngle = currentAngleDeg;

            if (useSteps && steps > 1)
            {
                float span = (maxDegrees - minDegrees);
                float stepDeg = span / (steps - 1);
                int idx = Mathf.RoundToInt((outAngle - minDegrees) / stepDeg);
                idx = Mathf.Clamp(idx, 0, steps - 1);
                outAngle = minDegrees + idx * stepDeg;

                float t = (float)idx / (steps - 1);
                if (invertOutput) t = 1f - t;
                OnValueChanged?.Invoke(t);
            }
            else
            {
                if (useSnap && snapStep > 0f)
                    outAngle = Mathf.Round(outAngle / snapStep) * snapStep;

                if (useLimits)
                    outAngle = Mathf.Clamp(outAngle, minDegrees, maxDegrees);

                float t = Mathf.InverseLerp(minDegrees, maxDegrees, outAngle);
                if (invertOutput) t = 1f - t;
                OnValueChanged?.Invoke(t);
            }

            // Simpan & apply sudut hasil snapping/steps
            currentAngleDeg = outAngle;
            ApplyKnobRotation(currentAngleDeg);
        }

        private void ApplyKnobRotation(float angleDeg)
        {
            knobTransform.rotation = Quaternion.AngleAxis(angleDeg, axisWorld) * zeroRotWorld;
        }

        private static Transform GetInteractorAttach(IXRSelectInteractor interactor, IXRInteractable forThis)
        {
            var t = interactor.GetAttachTransform(forThis);
            return t != null ? t : (interactor as Component).transform;
        }
    }
}
