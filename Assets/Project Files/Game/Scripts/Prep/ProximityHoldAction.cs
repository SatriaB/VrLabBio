using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    [RequireComponent(typeof(Collider))]
    public class ProximityHoldAction : MonoBehaviour
    {
        [Header("Filter")]
        [Tooltip("Nama collider anak yang wajib menyentuh zona (mis. 'Blade'). Kosongkan untuk terima apa saja.")]
        public string requiredChildName = "Blade";

        [Header("Syarat")] [Tooltip("Durasi minimal 'aktif' (tekan Activate) di dalam zona.")]
        public float requiredHoldSeconds = 0.4f;

        [Tooltip("Jarak gerak minimal alat selama aktif (meter).")]
        public float requiredMoveDistance = 0.08f;

        //[Header("Output")] public QuestSignalEmitter signalEmitter;
        public string signalOnComplete = "slide.sliced";
        public UnityEvent OnCompleted;

        bool isInside, completed;
        Transform toolRoot;
        XRInteractableActivateFlag flag;
        float activeTime, movedDistance;
        Vector3 lastPos;

        void Reset()
        {
            var c = GetComponent<Collider>();
            if (c) c.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (completed) return;
            if (!string.IsNullOrEmpty(requiredChildName) && other.name != requiredChildName) return;

            flag = other.transform.root.GetComponent<XRInteractableActivateFlag>();
            if (flag == null) return;

            toolRoot = other.transform.root;
            isInside = true;
            activeTime = 0f;
            movedDistance = 0f;
            lastPos = toolRoot.position;
            Debug.Log(other.name);
        }

        void OnTriggerExit(Collider other)
        {
            if (!isInside) return;
            if (!string.IsNullOrEmpty(requiredChildName) && other.name != requiredChildName) return;
            if (other.transform.root != toolRoot) return;
            
            Debug.Log(other.name);

            isInside = false;
            toolRoot = null;
            flag = null;
            activeTime = 0f;
            movedDistance = 0f;
        }

        void Update()
        {
            if (completed || !isInside || toolRoot == null || flag == null) return;

            if (flag.IsActiveNow())
            {
                activeTime += Time.deltaTime;
                var p = toolRoot.position;
                movedDistance += Vector3.Distance(p, lastPos);
                lastPos = p;
                
                Debug.Log(movedDistance);

                if (activeTime >= requiredHoldSeconds && movedDistance >= requiredMoveDistance)
                    Complete();
            }
        }

        void Complete()
        {
            if (completed) return;
            completed = true;
            OnCompleted?.Invoke();
            QuestEvents.Emit(signalOnComplete);
            /*if (signalEmitter && !string.IsNullOrEmpty(signalOnComplete))
                signalEmitter.Emit(signalOnComplete);*/
        }
    }
}