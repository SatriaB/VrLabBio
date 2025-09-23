using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    public class CaliperStationLocker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor stationSocket;

        [Header("Lock Strategy")]
        [Tooltip("Disable XRGrabInteractable component on body when docked.")]
        [SerializeField] private bool disableGrabComponent = true;

        [Tooltip("Alternatively, swap to a non-interactable layer when docked.")]
        [SerializeField] private bool swapInteractionLayer = false;

        [SerializeField] private InteractionLayerMask lockedInteractionLayer;

        public bool IsDocked { get; private set; }
        public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable CurrentCaliperGrab { get; private set; }

        private InteractionLayerMask _originalLayers;

        private void Reset()
        {
            stationSocket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
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
            CurrentCaliperGrab = args.interactableObject.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (CurrentCaliperGrab == null) return;

            _originalLayers = CurrentCaliperGrab.interactionLayers;

            if (disableGrabComponent) CurrentCaliperGrab.enabled = false;
            if (swapInteractionLayer) CurrentCaliperGrab.interactionLayers = lockedInteractionLayer;
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
        }
    }
}
