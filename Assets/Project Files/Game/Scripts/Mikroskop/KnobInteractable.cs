using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace FatahDev
{
    public class KnobInteractable : XRBaseInteractable
    {
        [Header("Target & Axis")]
        [SerializeField] private Transform knobTransform;
        [SerializeField] private Vector3 localAxis = Vector3.forward;
        [SerializeField] private bool axisIsLocal = true;

        [Header("Limits (deg)")]
        [SerializeField] private bool useLimits = true;
        [SerializeField] private float minDegrees = 0f;
        [SerializeField] private float maxDegrees = 90f;

        [Header("Snapping (optional)")]
        [SerializeField] private bool useSnap = false;
        [SerializeField] private float snapStep = 10f;

        [Header("Steps (optional)")]
        [SerializeField] private bool useSteps = false;   // nyalakan kalau mau diskrit
        [SerializeField] private int steps = 5;           // termasuk min & max

        [Header("Output")]
        [SerializeField] private bool invertOutput = false;   // kebalikin 0..1 → 1..0
        public UnityEvent<float> OnValueChanged;              // 0..1 (buat brightness)

        private IXRSelectInteractor selectingInteractor;
        private Quaternion initialKnobWorldRotation;
        private Vector3 initialDirOnPlane;
        private Vector3 axisWorld;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            selectingInteractor = args.interactorObject;

            axisWorld = (axisIsLocal ? knobTransform.TransformDirection(localAxis) : localAxis).normalized;

            var center = knobTransform.position;
            var hand = selectingInteractor.GetAttachTransform(this);

            initialDirOnPlane = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;
            initialKnobWorldRotation = knobTransform.rotation;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            selectingInteractor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || selectingInteractor == null)
                return;

            var center = knobTransform.position;
            var hand = selectingInteractor.GetAttachTransform(this);

            Vector3 currentDir = Vector3.ProjectOnPlane(hand.position - center, axisWorld).normalized;

            float delta = Vector3.SignedAngle(initialDirOnPlane, currentDir, axisWorld);

            float target = delta;

            if (useLimits)
                target = Mathf.Clamp(target, minDegrees, maxDegrees);

            float clamped = target;
            float tNorm;

            if (useSteps && steps > 1)
            {
                float span = (maxDegrees - minDegrees);
                float stepDeg = span / (steps - 1);
                int idx = Mathf.RoundToInt((clamped - minDegrees) / stepDeg);
                idx = Mathf.Clamp(idx, 0, steps - 1);

                target = minDegrees + idx * stepDeg;
                tNorm = (float)idx / (steps - 1);
            }
            else
            {
                if (useSnap && snapStep > 0f)
                {
                    target = Mathf.Round(target / snapStep) * snapStep;
                    clamped = Mathf.Clamp(target, minDegrees, maxDegrees);
                }

                tNorm = Mathf.InverseLerp(minDegrees, maxDegrees, clamped);
            }

            knobTransform.rotation = Quaternion.AngleAxis(target, axisWorld) * initialKnobWorldRotation;

            if (invertOutput) tNorm = 1f - tNorm;
            OnValueChanged?.Invoke(tNorm);
        }
    }
}
