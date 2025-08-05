using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    public class KnobInteractable : XRBaseInteractable
    {
        [SerializeField] private Transform knobTransform;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float snapAngle = 15f;

        private IXRSelectInteractor pullingInteractor;

        private Quaternion initialInteractorRotation;
        private Quaternion initialKnobRotation;
        private float totalRotation;

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && pullingInteractor != null)
            {
                Transform interactorTransform = pullingInteractor.GetAttachTransform(this).transform;

                Quaternion currentRotation = interactorTransform.rotation;
                Quaternion delta = currentRotation * Quaternion.Inverse(initialInteractorRotation);

                delta.ToAngleAxis(out float angle, out Vector3 axis);
                float direction = Vector3.Dot(axis, rotationAxis.normalized);
                float signedAngle = angle * Mathf.Sign(direction);

                knobTransform.rotation = initialKnobRotation * Quaternion.AngleAxis(signedAngle, rotationAxis);
                totalRotation = signedAngle;
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            pullingInteractor = args.interactorObject;

            initialInteractorRotation = pullingInteractor.GetAttachTransform(this).rotation;
            initialKnobRotation = knobTransform.rotation;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            pullingInteractor = null;

            // Snap saat dilepas
            float snapped = Mathf.Round(totalRotation / snapAngle) * snapAngle;
            float offset = snapped - totalRotation;

            knobTransform.Rotate(rotationAxis, offset, Space.Self);
            totalRotation = snapped;
        }
    }
}