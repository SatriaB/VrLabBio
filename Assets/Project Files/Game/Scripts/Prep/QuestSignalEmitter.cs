using UnityEngine;

namespace FatahDev
{
    public class QuestSignalEmitter : MonoBehaviour
    {
        public MonoBehaviour receiver;
        public string receiverMethodName = "Emit";
        [System.Serializable] public class StringEvent : UnityEngine.Events.UnityEvent<string> {}
        public StringEvent OnEmit;

        public void Emit(string eventName)
        {
            Debug.Log($"[QuestSignalEmitter] {eventName}");
            OnEmit?.Invoke(eventName);
            if (receiver && !string.IsNullOrEmpty(receiverMethodName))
                receiver.SendMessage(receiverMethodName, eventName, SendMessageOptions.DontRequireReceiver);
        }
    }
}