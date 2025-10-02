using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;   // XRGrabInteractable (v3)
using UnityEngine.XR.Interaction.Toolkit.Interactors;     // XRSocketInteractor (untuk tipe)
using UnityEngine.XR.Interaction.Toolkit;                 // XRInteractionManager (referensi optional)

namespace FatahDev
{
    /// <summary>
    /// Sendok XR yang bisa di-grab. Ujungnya (tip) bertindak sebagai trigger:
    /// - Sentuh gula (SugarBlock + XRGrabInteractable, tidak sedang dipegang) -> ambil satu, tempel ke tip.
    /// - Sentuh pan neraca (ada PanMultiSlotRouterTrigger di area) -> lepas gula supaya router men-seat ke slot.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class XRSpoonSugarTool : MonoBehaviour
    {
        [Header("Tip & Mount")]
        [SerializeField] private Transform tipTrigger;   // chilid dengan collider sTrigger=true
        [SerializeField] private Transform tipMount;     // posisi nempel gula di ujung sendok
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private Vector3 localEuler;
        [SerializeField] private bool scaleHeldToOne = true;

        [Header("Rules")]
        [SerializeField] private LayerMask sugarDetectLayers = ~0;
        [SerializeField] private bool requireSpoonGrabbed = false;   // bila true, hanya ambil saat sendok sedang digenggam
        [SerializeField] private bool holdOnlyOne = true;            // pegang 1 gula saja

        [Header("Physics while held")]
        [SerializeField] private bool makeHeldKinematic = true;      // saat menempel di sendok
        [SerializeField] private bool disableGravityWhileHeld = true;

        [Header("Refs (opsional)")]
        [SerializeField] private XRInteractionManager interactionManager; // tidak wajib

        // runtime
        private XRGrabInteractable _spoonGrab;
        private GameObject _heldPiece;
        private Rigidbody _heldRb;
        private Collider[] _heldCols;

        void Reset()
        {
            _spoonGrab = GetComponent<XRGrabInteractable>();
            if (!tipMount) tipMount = transform;
        }

        void Awake()
        {
            _spoonGrab = GetComponent<XRGrabInteractable>();
            if (!interactionManager) interactionManager = FindAnyObjectByType<XRInteractionManager>();

            // pastikan tipTrigger punya collider trigger
            if (tipTrigger)
            {
                var col = tipTrigger.GetComponent<Collider>();
                if (!col) col = tipTrigger.gameObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
            }
        }

        void OnEnable()
        {
            if (tipTrigger)
            {
                var hook = tipTrigger.gameObject.GetComponent<TriggerHook>();
                if (!hook) hook = tipTrigger.gameObject.AddComponent<TriggerHook>();
                hook.owner = this;
            }
        }

        // ---- Dipanggil oleh TriggerHook di tip ----
        internal void OnTipTriggerEnter(Collider other)  => HandleTipContact(other);
        internal void OnTipTriggerStay(Collider other)   => HandleTipContact(other);

        void HandleTipContact(Collider other)
        {
            if (((1 << other.gameObject.layer) & sugarDetectLayers.value) == 0) return;

            // 1) Lepas ke pan bila menyentuh area PanMultiSlotRouterTrigger
            if (_heldPiece && other.GetComponentInParent<PanMultiSlotRouterTrigger>())
            {
                ReleaseToWorld(); // lepas → router pan bakal seat ke slot kosong
                return;
            }

            // 2) Ambil gula kalau belum pegang
            if (holdOnlyOne && _heldPiece) return;
            if (requireSpoonGrabbed && !_spoonGrab.isSelected) return;

            // Cari SugarBlock + XRGrabInteractable pada collider yg kita sentuh
            var sugar = other.GetComponentInParent<SugarBlock>();
            if (!sugar) return;

            var sugarGrab = other.GetComponentInParent<XRGrabInteractable>();
            if (!sugarGrab) return;

            // Jangan ambil kalau gula sedang dipegang user/slot
            if (sugarGrab.isSelected) return; // router pan kamu pun abaikan yang masih dipegang :contentReference[oaicite:3]{index=3}

            // Ambil satu
            AttachPiece(sugarGrab.gameObject);
        }

        void AttachPiece(GameObject piece)
        {
            _heldPiece = piece;
            _heldRb    = piece.GetComponent<Rigidbody>();
            _heldCols  = piece.GetComponentsInChildren<Collider>(includeInactive: false);

            // Biarkan collider tetap AKTIF supaya trigger Pan bisa mendeteksi gula,
            // router akan "SelectEnter" ke slot terdekat setelah kita lepas. :contentReference[oaicite:4]{index=4}

            if (_heldRb)
            {
                if (makeHeldKinematic) _heldRb.isKinematic = true;
                if (disableGravityWhileHeld) _heldRb.useGravity = false;
                _heldRb.linearVelocity = Vector3.zero;
                _heldRb.angularVelocity = Vector3.zero;
            }

            piece.transform.SetParent(tipMount ? tipMount : transform, worldPositionStays: false);
            if (scaleHeldToOne) piece.transform.localScale = Vector3.one;
            piece.transform.localPosition = localOffset;
            piece.transform.localRotation = Quaternion.Euler(localEuler);
        }

        void ReleaseToWorld()
        {
            if (!_heldPiece) return;

            _heldPiece.transform.SetParent(null, true);

            if (_heldRb)
            {
                _heldRb.isKinematic = false;
                _heldRb.useGravity  = true;
            }

            _heldPiece = null;
            _heldRb    = null;
            _heldCols  = null;
        }

        // ---------------- tiny helper to forward trigger events ----------------
        // dipasang otomatis di tipTrigger
        private class TriggerHook : MonoBehaviour
        {
            public XRSpoonSugarTool owner;
            void OnTriggerEnter(Collider other) { owner?.OnTipTriggerEnter(other); }
            void OnTriggerStay(Collider other)  { owner?.OnTipTriggerStay(other); }
        }
    }
}
