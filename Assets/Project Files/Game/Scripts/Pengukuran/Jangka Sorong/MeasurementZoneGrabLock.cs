using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    public class MeasurementZoneGrabLock : MonoBehaviour
    {
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor measureZoneSocket;
        [Tooltip("Layer mask 'aman' yang TIDAK dibaca oleh interactor player (mis. 'Locked').")]
        [SerializeField] private InteractionLayerMask lockedMask;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;               // komponen grab di objek yang disocket
        private InteractionLayerMask _originalMask;     // simpan mask asli untuk dipulihkan

        private void Reset()
        {
            measureZoneSocket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (measureZoneSocket == null) return;
            measureZoneSocket.selectEntered.AddListener(OnEnter);
            measureZoneSocket.selectExited.AddListener(OnExit);

            // optional, biar socket nggak gampang lepas
            measureZoneSocket.keepSelectedTargetValid = true;
        }

        private void OnDisable()
        {
            if (measureZoneSocket == null) return;
            measureZoneSocket.selectEntered.RemoveListener(OnEnter);
            measureZoneSocket.selectExited.RemoveListener(OnExit);
        }

        private void OnEnter(SelectEnterEventArgs args)
        {
            _grab = args.interactableObject.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (_grab == null) return;

            _originalMask = _grab.interactionLayers;
            _grab.interactionLayers = lockedMask;   // kunci: player interactor tidak bisa select
            _grab.throwOnDetach = false;            // aman kalau nanti dilepas
        }

        private void OnExit(SelectExitEventArgs args)
        {
            if (_grab != null)
            {
                _grab.interactionLayers = _originalMask; // pulihkan normal
                _grab = null;
            }
        }
    }
}
