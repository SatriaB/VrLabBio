using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace FatahDev
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class XRInteractableActivateFlag : MonoBehaviour
    {
        [Tooltip("Lama 'aktif' setelah tekan Activate, untuk gating zona.")]
        public float activeWindow = 0.6f;

        private float _lastActivatedTime = -999f;
        private XRBaseInteractable _interactable;

        void Awake() => _interactable = GetComponent<XRBaseInteractable>();

        void OnEnable()
        {
            if (_interactable != null)
                _interactable.activated.AddListener(OnActivated);
        }

        void OnDisable()
        {
            if (_interactable != null)
                _interactable.activated.RemoveListener(OnActivated);
        }

        void OnActivated(ActivateEventArgs _)
        {
            Debug.Log("XRInteractableActivateFlag.OnActivated");
            _lastActivatedTime = Time.time;
        }

        public bool IsActiveNow()
        {
            return (Time.time - _lastActivatedTime) <= activeWindow;
        }
    }
}