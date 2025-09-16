using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    /// <summary>
    /// Event hub untuk fase PREP: slice + water drop + cover slip.
    /// Letakkan satu komponen ini di scene (misal di "MicroscopeRoot").
    /// </summary>
    public class AssembleSlideEventHub : MonoBehaviour
    {
        [Header("State (read-only)")]
        [SerializeField] private bool sliceDone;
        [SerializeField] private bool waterDropped;
        [SerializeField] private bool coverSlipPlaced;

        public bool SliceDone => sliceDone;
        public bool WaterDropped => waterDropped;
        public bool CoverSlipPlaced => coverSlipPlaced;

        [Header("Events")]
        public UnityEvent OnSliceDone;
        public UnityEvent OnWaterDropped;
        public UnityEvent OnCoverSlipPlaced;

        // ==== Pemicu dari interaksi (XR, trigger, tombol, dll) ====
        public void MarkSliceDone()
        {
            if (sliceDone) return;
            sliceDone = true;
            OnSliceDone?.Invoke();
            Debug.Log("[AssembleSlideEventHub] Slice DONE");
        }

        public void MarkWaterDropped()
        {
            if (waterDropped) return;
            waterDropped = true;
            OnWaterDropped?.Invoke();
            Debug.Log("[AssembleSlideEventHub] Water drop DONE");
        }

        public void MarkCoverSlipPlaced()
        {
            if (coverSlipPlaced) return;
            coverSlipPlaced = true;
            OnCoverSlipPlaced?.Invoke();
            Debug.Log("[AssembleSlideEventHub] Cover slip DONE");
        }

        // Opsional kalau mau reset dari UI
        [ContextMenu("Reset Flags")]
        public void ResetFlags()
        {
            sliceDone = false;
            waterDropped = false;
            coverSlipPlaced = false;
        }
    }
}