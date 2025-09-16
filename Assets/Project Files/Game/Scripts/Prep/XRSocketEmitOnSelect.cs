using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class XRSocketEmitOnSelect : MonoBehaviour
    {
        //public QuestSignalEmitter signalEmitter;
        public string signalOnSelect = "slide.cover_applied";
        public string signalOnExit = "";

        XRSocketInteractor socket;

        void Awake() => socket = GetComponent<XRSocketInteractor>();

        void OnEnable()
        {
            if (socket != null) socket.selectEntered.AddListener(OnSelectEntered);
            if (socket != null) socket.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            if (socket != null) socket.selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnSelectEntered(SelectEnterEventArgs _)
        {
            if (!string.IsNullOrEmpty(signalOnSelect)) QuestEvents.Emit(signalOnSelect);
        }
        
        void OnSelectExited(SelectExitEventArgs  _)
        {
            if (!string.IsNullOrEmpty(signalOnExit)) QuestEvents.Emit(signalOnExit);
        }
    }
}