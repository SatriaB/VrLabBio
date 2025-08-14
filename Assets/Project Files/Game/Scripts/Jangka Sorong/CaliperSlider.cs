using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace NusaForge.VR
{
    [RequireComponent(typeof(Collider))]
    public class CaliperSlider : XRBaseInteractable
    {
        public enum LimitMode { Anchors, Axis }
        [Header("Limit Mode")]
        public LimitMode limitMode = LimitMode.Axis;

        [Header("Anchors (recommended)")]
        public Transform anchorA; // t=0
        public Transform anchorB; // t=max

        [Header("Axis mode")]
        public Transform reference;
        public Vector3 localAxis = Vector3.up; // contoh: geser sumbu Y
        public float min = 0f;    // meter (tidak dipakai jika useEditorPositionAsZero = true)
        public float max = 0.15f; // meter (tetap dipakai sebagai batas atas)

        [Header("Axis Zero (Axis mode)")]
        [Tooltip("Jadikan posisi editor sebagai 0 mm (tanpa menggeser objek saat start).")]
        public bool useEditorPositionAsZero = true;

        [Header("Output & Tuning")]
        public float snapStepMm = 0f;     // 0 = no snap
        public float deadzone = 0.00005f; // anti jitter
        public float currentMm;           // bacaan mm dari 0 editor
        public UnityEvent<float> onValueChangedMm;

        // --- runtime state ---
        private IXRSelectInteractor pulling;
        private float deltaT;                    // offset slide vs tangan
        private float lastEmittedMm = float.NaN;
        private Vector3 aW, bW, axisW;          // rail world
        private float length;                    // meter (maxEff - minEff)
        private float _editorZeroT;              // meter (pos editor di sepanjang sumbu, rel. ke reference)
        private bool _zeroInitialized;

        protected override void Awake()
        {
            base.Awake();
            selectMode = InteractableSelectMode.Single;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!reference) reference = transform.parent ? transform.parent : transform;

            // Hitung posisi editor sebagai nol (hanya sekali, tidak menggeser objek)
            if (limitMode == LimitMode.Axis && useEditorPositionAsZero)
            {
                Vector3 nLocal = localAxis.normalized;
                Vector3 localPos = reference.InverseTransformPoint(transform.position);
                _editorZeroT = Vector3.Dot(localPos, nLocal); // meter
                _zeroInitialized = true;
            }

            RecalcRail(); // rail pakai minEff = editorZero atau 'min'

            // Jangan geser posisi; cukup update bacaan
            float tNow = GetSlideTWorld(); // 0 saat di posisi editor (jika useEditorPositionAsZero)
            EmitIfChanged(tNow);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
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

            // deadzone
            float cur = Mathf.Clamp(GetSlideTWorld(), 0f, length);
            if (Mathf.Abs(target - cur) < deadzone) target = cur;

            // snap (mm -> m)
            if (snapStepMm > 0.0001f)
            {
                float stepM = snapStepMm / 1000f;
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

            // Axis mode
            if (!reference) reference = transform.parent ? transform.parent : transform;

            Vector3 origin = reference.position;
            axisW = reference.TransformDirection(localAxis.normalized);

            // min efektif = posisi editor (jika diaktifkan), max tetap tidak berubah
            float minEff = (useEditorPositionAsZero && _zeroInitialized) ? _editorZeroT : min;
            float maxEff = max;

            // jaga agar tidak kebalik
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
            transform.position = aW + axisW * t; // kinematik tepat di rel
        }

        private void EmitIfChanged(float tMetersFromZero)
        {
            // Karena aW sudah diposisikan di "nol editor", mm = t * 1000
            float mm = tMetersFromZero * 1000f;
            currentMm = mm;

            if (!Mathf.Approximately(mm, lastEmittedMm))
            {
                lastEmittedMm = mm;
                onValueChangedMm?.Invoke(mm);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // gambar rel aktual (sesudah zero editor diterapkan)
            if (!reference) reference = transform.parent ? transform.parent : transform;
            RecalcRail();
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(aW, bW);
            Gizmos.DrawWireSphere(aW, 0.002f);
            Gizmos.DrawWireSphere(bW, 0.002f);
        }
#endif
    }
}
