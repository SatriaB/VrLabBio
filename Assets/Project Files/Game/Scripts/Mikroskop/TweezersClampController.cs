using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace FatahDev
{
    public class TweezersClampController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private XRGrabInteractable grabInteractable;
        [SerializeField] private Animator animator;

        [Header("Animator")]
        [SerializeField] private string isClosedParam = "IsClosed";
        [SerializeField] private bool requireGrabToActivate = true;

        // ============== Tambahan minimal untuk mekanik ambil & drop ==============
        [Header("Clamp Pickup")]
        [SerializeField] private Transform clampAnchor;            // empty di ujung capit
        [SerializeField] private float clampRadius = 0.02f;        // jangkauan cari sampel
        [SerializeField] private LayerMask specimenLayerMask;      // layer untuk "dummy sample"

        [Header("Slide Drop (simple)")]
        [SerializeField] private float zoneCheckRadius = 0.03f;    // radius cek zona slide
        [SerializeField] private LayerMask slideZoneLayerMask;     // layer untuk zona slide
        [SerializeField] private Transform slideSnapPoint;         // titik snap (child di kaca). optional

        [Header("Release Feel")]
        [SerializeField] private float inheritVelocityFactor = 0.8f;
        [SerializeField] private UnityEvent onPlaced;

        // runtime
        private Transform currentSpecimen;
        private Rigidbody currentBody;
        private readonly List<Collider> currentCols = new List<Collider>(8);
        private readonly Collider[] overlap = new Collider[8];
        private Vector3 prevPos, vel;

        private void Awake()
        {
            if (!grabInteractable) grabInteractable = GetComponent<XRGrabInteractable>();
            if (!animator) animator = GetComponent<Animator>();
        }

        private void Update()
        {
            vel = (transform.position - prevPos) / Mathf.Max(Time.deltaTime, 0.0001f);
            prevPos = transform.position;
        }

        public void OnActivated()
        {
            if (requireGrabToActivate && (grabInteractable == null || !grabInteractable.isSelected))
                return;

            if (animator) animator.SetBool(isClosedParam, true);   

            if (currentSpecimen != null) return;

            int n = Physics.OverlapSphereNonAlloc(
                clampAnchor ? clampAnchor.position : transform.position,
                clampRadius, overlap, specimenLayerMask, QueryTriggerInteraction.Collide);

            Collider bestCol = null;
            float bestSqr = float.PositiveInfinity;

            for (int i = 0; i < n; i++)
            {
                var col = overlap[i];
                if (!col) continue;

                float sq = (col.transform.position - (clampAnchor ? clampAnchor.position : transform.position)).sqrMagnitude;
                if (sq < bestSqr)
                {
                    bestSqr = sq;
                    bestCol = col;
                }
            }

            if (bestCol == null) return;

            AttachSpecimen(bestCol.transform);
        }

        public void OnDeactivated()
        {
            if (requireGrabToActivate && (grabInteractable == null || !grabInteractable.isSelected))
                return;

            if (animator) animator.SetBool(isClosedParam, false);  // anim buka

            if (currentSpecimen == null) return;

            // Cek apakah ujung pinset cukup dekat dengan slideSnapPoint
            bool overSlide = slideSnapPoint != null &&
                             Vector3.Distance((clampAnchor ? clampAnchor.position : transform.position),
                                 slideSnapPoint.position) <= zoneCheckRadius;

            if (overSlide)
            {
                DetachToSlide(slideSnapPoint); // <-- pakai snapPoint yang sudah ada
                return;
            }
            DetachToWorld();
        }

        // ========================= Helper attach/detach =========================
        private void AttachSpecimen(Transform specimen)
        {
            currentSpecimen = specimen;
            currentCols.Clear();
            specimen.GetComponentsInChildren(true, currentCols);

            currentBody = specimen.GetComponent<Rigidbody>();
            if (currentBody != null)
            {
                currentBody.isKinematic = true;
                currentBody.linearVelocity = Vector3.zero;
                currentBody.angularVelocity = Vector3.zero;
            }

            foreach (var c in currentCols) if (c) c.isTrigger = true;

            var anchor = clampAnchor ? clampAnchor : transform;
            specimen.SetParent(anchor, false);
            specimen.localPosition = Vector3.zero;
            specimen.localRotation = Quaternion.identity;
            
            QuestEvents.Emit(QuestSignals.PINSET_SAMPLE_PICKED);
        }

        private void DetachToSlide(Transform snapPoint)
        {
            var t = currentSpecimen;
            currentSpecimen = null;

            // Parent ke SLIDE (pakai parent dari snapPoint)
            var slideParent = snapPoint.parent != null ? snapPoint.parent : snapPoint;
            t.SetParent(slideParent, true);

            // Snap pos/rot persis ke snapPoint
            t.position = snapPoint.position;
            t.rotation = snapPoint.rotation;

            // Pulihkan collider & kunci fisika di atas slide
            foreach (var c in currentCols) if (c) c.isTrigger = false;

            if (currentBody != null)
            {
                currentBody.isKinematic = true;
                currentBody.linearVelocity = Vector3.zero;
                currentBody.angularVelocity = Vector3.zero;
            }

            currentBody = null;
            currentCols.Clear();
            
            QuestEvents.Emit(QuestSignals.SAMPLE_PLACED_ON_SLIDE);
            onPlaced.Invoke();
        }

        private void DetachToWorld()
        {
            var t = currentSpecimen;
            currentSpecimen = null;

            t.SetParent(null, true);

            foreach (var c in currentCols) if (c) c.isTrigger = false;

            if (currentBody != null)
            {
                currentBody.isKinematic = false;
                currentBody.linearVelocity = vel * inheritVelocityFactor;
            }

            currentBody = null;
            currentCols.Clear();
        }
    }
}
