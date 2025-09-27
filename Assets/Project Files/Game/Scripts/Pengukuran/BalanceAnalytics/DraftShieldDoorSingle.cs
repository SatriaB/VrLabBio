using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class DraftShieldDoorSingle : XRBaseInteractable
    {
        [Header("Target Pintu")]
        [SerializeField] private Transform doorTransform;   // biasanya transform ini sendiri
        [SerializeField] private Transform doorParent;      // parent lokal pintu (body neraca)

        [Header("Batas Lokal X (meter, relatif ke doorParent)")]
        [SerializeField] private float closedLocalX = 0f;   // posisi tertutup
        [SerializeField] private float openLocalX = 0.14f;  // posisi terbuka (+14 cm)

        [Header("Lock & Smoothing")]
        [SerializeField] private bool lockLocalY = true;
        [SerializeField] private bool lockLocalZ = true;
        [SerializeField] private bool lockLocalRotation = true;
        [SerializeField] private float smoothLerp = 1f;     // 1 = instan; <1 halus

        [Header("Output (opsional)")]
        public UnityEvent<float> OnRatioChanged;            // 0..1

        public float DoorOpenRatio { get; private set; }

        private IXRSelectInteractor currentInteractor;
        private float minX, maxX;
        private Vector3 baseLocalPos;
        private Quaternion baseLocalRot;
        private float grabOffsetX;
        private float targetLocalX;

        protected override void Awake()
        {
            base.Awake();
            if (!doorTransform) doorTransform = transform;
            if (!doorParent)    doorParent    = transform.parent;

            selectMode = InteractableSelectMode.Single; // hanya 1 tangan yg boleh
            minX = Mathf.Min(closedLocalX, openLocalX);
            maxX = Mathf.Max(closedLocalX, openLocalX);

            baseLocalPos = doorTransform.localPosition;
            baseLocalRot = doorTransform.localRotation;
            targetLocalX = doorTransform.localPosition.x;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            currentInteractor = args.interactorObject;

            // Hitung offset biar gak "lompat" saat mulai geser
            Vector3 handLocal = doorParent.InverseTransformPoint(GetInteractorWorldPos(currentInteractor));
            float doorX = doorTransform.localPosition.x;
            float handX = handLocal.x;
            grabOffsetX = doorX - handX;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            currentInteractor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            // Update di phase Dynamic untuk respons gerak halus
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            Vector3 lp = doorTransform.localPosition;
            Quaternion lr = doorTransform.localRotation;

            if (currentInteractor != null)
            {
                Vector3 handLocal = doorParent.InverseTransformPoint(GetInteractorWorldPos(currentInteractor));
                float desiredX = handLocal.x + grabOffsetX;
                targetLocalX = Mathf.Clamp(desiredX, minX, maxX);
            }

            // Lerp opsional (anti jitter)
            if (smoothLerp >= 1f) lp.x = targetLocalX;
            else lp.x = Mathf.Lerp(lp.x, targetLocalX, 1f - Mathf.Pow(1f - smoothLerp, Time.deltaTime * 60f));

            if (lockLocalY) lp.y = baseLocalPos.y;
            if (lockLocalZ) lp.z = baseLocalPos.z;
            if (lockLocalRotation) lr = baseLocalRot;

            doorTransform.localPosition = lp;
            doorTransform.localRotation = lr;

            float ratio = Mathf.InverseLerp(closedLocalX, openLocalX, lp.x);
            if (Mathf.Abs(ratio - DoorOpenRatio) > 0.0001f)
            {
                DoorOpenRatio = ratio;
                Debug.Log("DoorOpenRatio: " + ratio);
                OnRatioChanged?.Invoke(ratio);
            }
        }

        private static Vector3 GetInteractorWorldPos(IXRSelectInteractor interactor)
        {
            // Attach transform kalau ada; fallback ke transform interactor
            var t = interactor.GetAttachTransform(null);
            return t ? t.position : (interactor as Object as Component).transform.position;
        }
    }
}
