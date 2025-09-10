using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class XRSocketEmitOnSelect : MonoBehaviour
    {
        public QuestSignalEmitter signalEmitter;
        public string signalOnSelect = "slide.cover_applied";

        XRSocketInteractor socket;

        void Awake() => socket = GetComponent<XRSocketInteractor>();

        void OnEnable()
        {
            if (socket != null) socket.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (socket != null) socket.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnSelectEntered(SelectEnterEventArgs _)
        {
            if (signalEmitter && !string.IsNullOrEmpty(signalOnSelect))
                signalEmitter.Emit(signalOnSelect);
        }
    }
}