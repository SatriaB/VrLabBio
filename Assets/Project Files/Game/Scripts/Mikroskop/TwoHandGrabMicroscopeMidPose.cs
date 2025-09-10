using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace FatahDev
{
    [RequireComponent(typeof(Rigidbody))]
    public class TwoHandGrabMicroscopeMidPose : XRGrabInteractable
    {
        [Header("Single-hand feedback")]
        [SerializeField] bool vibrateOnSingleHand = true;
        [SerializeField, Range(0f, 1f)] float hapticAmplitude = 0.35f;
        [SerializeField] float hapticDuration = 0.05f;

        Rigidbody rb;
        Transform dynamicAttach;

        protected override void Awake()
        {
            base.Awake();
            selectMode = InteractableSelectMode.Multiple;

            rb = GetComponent<Rigidbody>();
            dynamicAttach = new GameObject("DynamicAttach").transform;
            dynamicAttach.SetParent(transform, false);
            attachTransform = dynamicAttach;

            // Pastikan diawali tidak tracking (butuh 2 tangan dahulu)
            trackPosition = false;
            trackRotation = false;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (interactorsSelecting.Count == 1 && vibrateOnSingleHand)
                TryHaptic(args.interactorObject as IXRInteractor);

            UpdateTrackingState();
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            UpdateTrackingState();
        }

        void UpdateTrackingState()
        {
            bool twoHands = interactorsSelecting.Count >= 2;
            trackPosition = twoHands;
            trackRotation = twoHands;

            if (!twoHands && rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        void LateUpdate()
        {
            if (!isSelected || interactorsSelecting.Count < 2) return;

            // Ambil 2 interactor pertama
            IXRSelectInteractor A = interactorsSelecting[0];
            IXRSelectInteractor B = interactorsSelecting[1];

            Transform aT = GetAttach(A);
            Transform bT = GetAttach(B);

            Vector3 mid = (aT.position + bT.position) * 0.5f;
            Vector3 right = (bT.position - aT.position);
            if (right.sqrMagnitude < 1e-6f) right = transform.right;
            else right.Normalize();

            // Up = rata-rata up dua controller, fallback ke world up
            Vector3 up = (aT.up + bT.up) * 0.5f;
            if (up.sqrMagnitude < 1e-6f) up = Vector3.up;
            else up.Normalize();

            // Forward = right x up (ortho)
            Vector3 fwd = Vector3.Cross(right, up);
            if (fwd.sqrMagnitude < 1e-6f)
            {
                // fallback: proyeksi forward eksisting ke bidang up
                fwd = Vector3.ProjectOnPlane(transform.forward, up);
                if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.Cross(right, up);
            }
            fwd.Normalize();

            Quaternion rot = Quaternion.LookRotation(fwd, up);
            dynamicAttach.SetPositionAndRotation(mid, rot);
        }

        Transform GetAttach(IXRSelectInteractor interactor)
        {
            var asInteractor = interactor as IXRInteractor;
            return asInteractor != null
                ? asInteractor.GetAttachTransform(this)
                : (interactor as Component)?.transform;
        }

        void TryHaptic(IXRInteractor interactor)
        {
            if (interactor is XRBaseInputInteractor c)
                c.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }
    }
}
