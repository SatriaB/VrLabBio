using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    [RequireComponent(typeof(Collider))]
    public class ProximityButtonAction : MonoBehaviour
    {
        [Tooltip("Nama collider anak yang wajib menyentuh zona (mis. 'Nozzle').")]
        public string requiredChildName = "Nozzle";

        [Tooltip("Cooldown agar tidak spam (detik).")]
        public float cooldown = 1f;

        //[Header("Output")] public QuestSignalEmitter signalEmitter;
        public string signalOnPress = "slide.water_dropped";
        public UnityEvent OnPressed;

        float lastFireTime = -999f;

        void Reset()
        {
            var c = GetComponent<Collider>();
            if (c) c.isTrigger = true;
        }

        void OnTriggerStay(Collider other)
        {
            if (Time.time < lastFireTime + cooldown) return;
            if (!string.IsNullOrEmpty(requiredChildName) && other.name != requiredChildName) return;
            
            var flag = other.transform.root.GetComponent<XRInteractableActivateFlag>();
            if (flag != null && flag.IsActiveNow())
            {
                lastFireTime = Time.time;
                OnPressed?.Invoke();
                QuestEvents.Emit(signalOnPress);
            }
        }
    }
}