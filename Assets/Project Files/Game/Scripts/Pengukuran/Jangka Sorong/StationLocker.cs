using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    public class StationLocker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private XRSocketInteractor stationSocket;

        [Header("Lock Strategy")]
        [SerializeField] private bool disableGrabComponent = true;
        [SerializeField] private bool swapInteractionLayer = false;
        [SerializeField] private InteractionLayerMask lockedInteractionLayer;

        // ====== Tambahan untuk Quest ======
        public enum StationInstrumentKind { None, Caliper, Micrometer, Balance }

        [Header("Quest Bridge")]
        [Tooltip("Jenis alat yang diparkir di station ini")]
        [SerializeField] private StationInstrumentKind instrumentKind = StationInstrumentKind.None;

        [Tooltip("Emit sinyal quest saat alat masuk station")]
        [SerializeField] private bool emitSignalOnDock = true;

        [Tooltip("Override sinyal dock (kosongkan untuk otomatis sesuai instrumentKind)")]
        [SerializeField] private string overrideDockSignal = "";

        [Tooltip("Emit sinyal quest saat alat keluar station (opsional)")]
        [SerializeField] private bool emitSignalOnUndock = false;

        [Tooltip("Override sinyal undock (opsional)")]
        [SerializeField] private string overrideUndockSignal = "";

        [Header("Unity Events (opsional untuk UI/FX)")]
        public UnityEvent OnDocked;
        public UnityEvent OnUndocked;

        public bool IsDocked { get; private set; }
        public XRGrabInteractable CurrentCaliperGrab { get; private set; }

        private InteractionLayerMask _originalLayers;

        private void Reset()
        {
            stationSocket = GetComponent<XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (stationSocket == null) return;
            stationSocket.selectEntered.AddListener(OnSelectEntered);
            stationSocket.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            if (stationSocket == null) return;
            stationSocket.selectEntered.RemoveListener(OnSelectEntered);
            stationSocket.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            IsDocked = true;
            CurrentCaliperGrab = args.interactableObject.transform.GetComponent<XRGrabInteractable>();
            if (CurrentCaliperGrab != null)
            {
                _originalLayers = CurrentCaliperGrab.interactionLayers;
                if (disableGrabComponent) CurrentCaliperGrab.enabled = false;
                if (swapInteractionLayer) CurrentCaliperGrab.interactionLayers = lockedInteractionLayer;
            }

            // === Emit sinyal QUEST untuk step "place"
            if (emitSignalOnDock)
            {
                string sig = !string.IsNullOrEmpty(overrideDockSignal)
                    ? overrideDockSignal
                    : AutoDockSignal();
                if (!string.IsNullOrEmpty(sig))
                    QuestEvents.Emit(sig);
            }

            OnDocked?.Invoke();
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            IsDocked = false;

            if (CurrentCaliperGrab != null)
            {
                if (swapInteractionLayer) CurrentCaliperGrab.interactionLayers = _originalLayers;
                if (disableGrabComponent) CurrentCaliperGrab.enabled = true;
            }
            CurrentCaliperGrab = null;

            // (opsional) emit sinyal saat undock
            if (emitSignalOnUndock && !string.IsNullOrEmpty(overrideUndockSignal))
                QuestEvents.Emit(overrideUndockSignal);

            OnUndocked?.Invoke();
        }

        // Pilih sinyal otomatis berdasarkan jenis alat
        private string AutoDockSignal()
        {
            switch (instrumentKind)
            {
                case StationInstrumentKind.Caliper:
                    return QuestSignals.CALIPER_SPECIMEN_PLACED;
                case StationInstrumentKind.Micrometer:
                    return QuestSignals.MICROMETER_PLACED;
                case StationInstrumentKind.Balance:
                    return QuestSignals.BALANCE_CONTAINER_PLACED;

                default:
                    return null;
            }
        }
    }
}
