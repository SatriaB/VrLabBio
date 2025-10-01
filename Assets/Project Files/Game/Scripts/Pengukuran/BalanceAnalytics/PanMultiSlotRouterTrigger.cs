using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    [RequireComponent(typeof(Collider))]
    public class PanMultiSlotRouterTrigger : MonoBehaviour
    {
        [Header("Tujuan (slot-slot kecil di pan)")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] targetSlots;

        [Header("XR Manager (auto-find kalau kosong)")]
        [SerializeField] private XRInteractionManager interactionManager;

        [Header("Seat Options")]
        [Tooltip("Jeda minimal antar seat untuk objek yang sama (detik) agar tidak spam).")]
        [SerializeField] private float seatCooldownSeconds = 0.15f;
        [Tooltip("Batas jarak seat (meter). 0 = tanpa batas (pakai trigger volume saja).")]
        [SerializeField] private float maxSeatDistance = 0.0f;

        [Header("Counting (opsional)")]
        [SerializeField] private bool enableCounting = true;
        public UnityEvent<int, float> OnCountAndMassChanged;
        
        [SerializeField] private GenericCaptureProvider captureProvider;

        public int SugarCount { get; private set; }
        public float TotalMassGram { get; private set; }

        private readonly Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable, float> lastSeatTime = new();

        private void Reset()
        {
            var c = GetComponent<Collider>();
            c.isTrigger = true; // WAJIB trigger
        }

        private void Awake()
        {
            if (!interactionManager) interactionManager = FindAnyObjectByType<XRInteractionManager>();

            if (enableCounting && targetSlots != null)
            {
                foreach (var s in targetSlots)
                {
                    if (!s) continue;
                    s.selectEntered.AddListener(OnSlotChanged);
                    s.selectExited.AddListener(OnSlotChanged);
                }
                // Hitung awal
                RecountAndEmit();
            }
        }

        private void OnDestroy()
        {
            if (targetSlots != null)
            {
                foreach (var s in targetSlots)
                {
                    if (!s) continue;
                    s.selectEntered.RemoveListener(OnSlotChanged);
                    s.selectExited.RemoveListener(OnSlotChanged);
                }
            }
        }

        private void OnTriggerEnter(Collider other) => TrySeat(other);
        private void OnTriggerStay(Collider other)  => TrySeat(other);

        private void TrySeat(Collider col)
        {
            var sugarGrab = col.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (sugarGrab == null) return;
            if (!sugarGrab.GetComponentInParent<SugarBlock>()) return; // pastikan ini gula
            if (sugarGrab.isSelected) return; // masih dipegang, jangan seat

            var interactable = (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)sugarGrab;

            if (lastSeatTime.TryGetValue(interactable, out float t))
            {
                if (Time.unscaledTime - t < seatCooldownSeconds) return; // anti-spam
            }

            var best = FindNearestEmptySlot(sugarGrab.transform.position);
            if (best == null) return;

            if (maxSeatDistance > 0f)
            {
                var a = best.attachTransform ? best.attachTransform.position : best.transform.position;
                if (Vector3.Distance(a, sugarGrab.transform.position) > maxSeatDistance) return;
            }

            if (!interactionManager) interactionManager = FindAnyObjectByType<XRInteractionManager>();
            
            CaptureRouter.SetActiveProvider(captureProvider);
            interactionManager.SelectEnter(best, interactable);

            lastSeatTime[interactable] = Time.unscaledTime;

            if (enableCounting) RecountAndEmit(); // update setelah seat
        }

        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor FindNearestEmptySlot(Vector3 fromWorldPos)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < targetSlots.Length; i++)
            {
                var s = targetSlots[i];
                if (!s || s.hasSelection) continue;

                var p = s.attachTransform ? s.attachTransform.position : s.transform.position;
                float d2 = (p - fromWorldPos).sqrMagnitude;
                if (d2 < bestSqr) { bestSqr = d2; best = s; }
            }
            return best;
        }

        private void OnSlotChanged(SelectEnterEventArgs _)
        {
            if (enableCounting) RecountAndEmit();
        }
        private void OnSlotChanged(SelectExitEventArgs _)
        {
            if (enableCounting) RecountAndEmit();
        }

        private void RecountAndEmit()
        {
            int count = 0;
            float mass = 0f;

            if (targetSlots != null)
            {
                for (int i = 0; i < targetSlots.Length; i++)
                {
                    var s = targetSlots[i];
                    if (s == null || !s.hasSelection) continue;

                    var list = s.interactablesSelected;
                    if (list.Count == 0) continue;

                    var comp = (list[0] as Object as Component);
                    var sugar = comp ? comp.GetComponentInParent<SugarBlock>() : null;
                    if (sugar != null)
                    {
                        count++;
                        mass += sugar.massGram;
                    }
                }
                
            }

            SugarCount = count;
            TotalMassGram = mass;

            if (targetSlots != null && SugarCount >= targetSlots.Length)
            {
                QuestEvents.Emit(QuestSignals.BALANCE_SAMPLE_PLACED);
            }

            OnCountAndMassChanged?.Invoke(SugarCount, TotalMassGram);
        }

        // Opsional buat diakses dari luar
        public void GetMassAndCount(out float totalMassGram, out int count)
        {
            totalMassGram = TotalMassGram;
            count = SugarCount;
        }
    }
}
