using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [RequireComponent(typeof(Collider))]
    public class CaliperSliderBase : XRBaseInteractable
    {
        public enum LimitMode { Anchors, Axis }
        [Header("Limit Mode")]
        public LimitMode limitMode = LimitMode.Axis;

        [Header("Anchors (opsional)")]
        public Transform anchorA; // t=0
        public Transform anchorB; // t=max

        [Header("Axis mode")]
        public Transform reference;
        public Vector3 localAxis = Vector3.right;
        public float max = 0.15f; // meter, panjang rail Unity
        
        [Header("Calibration")]
        [Tooltip("Panjang real rail caliper sesuai skala (cm). Contoh: 15 cm, 20 cm.")]
        public float realLengthCm = 15f;

        [Tooltip("Gunakan posisi editor sebagai 0 cm.")]
        public bool useEditorPositionAsZero = true;

        [Header("Output & Tuning")]
        public float snapStepCm = 0f;        // kelipatan cm
        public float deadzone = 0.00005f;    // meter
        public float currentCm;              // bacaan dalam cm
        public UnityEvent<float> onValueChangedCm;

        private IXRSelectInteractor pulling;
        private float deltaT;
        private float lastEmittedCm = float.NaN;
        private Vector3 aW, bW, axisW;
        private float length;             // panjang rail Unity (m)
        private float _editorZeroT;       // posisi editor (m)
        private bool _zeroInitialized;
        
        [SerializeField] private GenericCaptureProvider captureProvider;

        protected override void Awake()
        {
            base.Awake();
            selectMode = InteractableSelectMode.Single;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!reference) reference = transform.parent ? transform.parent : transform;

            if (limitMode == LimitMode.Axis && useEditorPositionAsZero)
            {
                Vector3 nLocal = localAxis.normalized;
                Vector3 localPos = reference.InverseTransformPoint(transform.position);
                _editorZeroT = Vector3.Dot(localPos, nLocal);
                _zeroInitialized = true;
            }

            RecalcRail();
            float tNow = GetSlideTWorld();
            EmitIfChanged(tNow);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            CaptureRouter.SetActiveProvider(captureProvider);
            pulling = args.interactorObject;
            RecalcRail();

            float slideT = Mathf.Clamp(GetSlideTWorld(), 0f, length);
            float handT  = ProjectHandTWorld();
            deltaT = slideT - handT;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            pulling = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || pulling == null)
                return;

            RecalcRail();

            float handT  = ProjectHandTWorld();
            float target = Mathf.Clamp(handT + deltaT, 0f, length);

            float cur = Mathf.Clamp(GetSlideTWorld(), 0f, length);
            if (Mathf.Abs(target - cur) < deadzone) target = cur;

            if (snapStepCm > 0.0001f)
            {
                float stepM = snapStepCm / 100f; // cm → meter
                target = Mathf.Round(target / stepM) * stepM;
            }

            SetSlideTWorld(target);
            EmitIfChanged(target);
        }

        // ================= Helpers =================
        private void RecalcRail()
        {
            if (limitMode == LimitMode.Anchors && anchorA && anchorB)
            {
                aW = anchorA.position;
                bW = anchorB.position;
                axisW = (bW - aW);
                length = axisW.magnitude;
                axisW = length > 1e-6f ? axisW / length : Vector3.right;
                return;
            }

            if (!reference) reference = transform.parent ? transform.parent : transform;

            Vector3 origin = reference.position;
            axisW = reference.TransformDirection(localAxis.normalized);

            float minEff = (useEditorPositionAsZero && _zeroInitialized) ? _editorZeroT : 0f;
            float maxEff = max;
            if (maxEff < minEff) maxEff = minEff;

            aW = origin + axisW * minEff;
            bW = origin + axisW * maxEff;
            length = Mathf.Max(0f, maxEff - minEff);
        }

        private float ProjectHandTWorld()
        {
            Transform attach = pulling.GetAttachTransform(this);
            return Vector3.Dot(attach.position - aW, axisW);
        }

        private float GetSlideTWorld()
        {
            return Vector3.Dot(transform.position - aW, axisW);
        }

        private void SetSlideTWorld(float t)
        {
            transform.position = aW + axisW * t;
        }

        private void EmitIfChanged(float tMetersFromZero)
        {
            // mapping meter → cm sesuai panjang rail real
            float cm = (length <= 1e-6f) ? 0f : (tMetersFromZero / length) * realLengthCm;
            currentCm = cm;

            if (!Mathf.Approximately(cm, lastEmittedCm))
            {
                lastEmittedCm = cm;
                onValueChangedCm?.Invoke(cm);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            RecalcRail();
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(aW, bW);
            Gizmos.DrawWireSphere(aW, 0.002f);
            Gizmos.DrawWireSphere(bW, 0.002f);
        }
#endif
    }
}
