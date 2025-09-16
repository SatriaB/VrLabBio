using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    // ======================= Quest Runner =======================
    /// <summary>
    /// Menjalankan urutan quest mikroskop (ringkas): Prep → Place → 4× → 10× → 40× → 100× (Complete).
    /// UI hooks disediakan untuk bind ke checklist UI.
    /// </summary>
    [DefaultExecutionOrder(-5000)]
    public class MicroscopeQuestRunner : MonoBehaviour
    {
        [Header("Runner Settings")]
        [Tooltip("Start quest automatically on play")]
        public bool autoStart = true;

        [Tooltip("Print step-by-step logs to Console")]
        public bool logProgress = true;

        // === UI HOOKS (untuk bind di Inspector) ===
        [System.Serializable] public class StepStartedEvent  : UnityEvent<string> {}       // stepId
        [System.Serializable] public class StepFinishedEvent : UnityEvent<string,bool> {}  // stepId, isDone

        [Header("UI Hooks (bind di Inspector)")]
        public UnityEvent OnQuestBuilt;
        public StepStartedEvent OnStepStarted;
        public StepFinishedEvent OnStepFinished;
        public UnityEvent OnQuestCompleted;

        // ===== Catalog (optional) =====
        [Header("Catalog (optional)")]
        [SerializeField] private MicroscopeQuestCatalog questCatalog;  // assign asset untuk edit via Inspector
        [SerializeField] private bool useBuiltInDefaultCatalog = true; // uncheck jika hanya mau pakai asset

        // ===== Default array (dipakai jika asset kosong / tidak diassign) =====
        private static readonly QuestSpec[] DefaultCatalog = new[]
        {
            new QuestSpec { id="prep_slide",   title="Prepare the slide",          kind=GoalKind.AssembleSlide, requireSlice=true, requireWaterDrop=true, requireCoverSlip=false },
            new QuestSpec { id="place_slide",  title="Place the slide on stage",   kind=GoalKind.PlaceSlide },
            new QuestSpec { id="set_4x",       title="Rotate to 4× objective",     kind=GoalKind.SetTurret, objective=4 },
            new QuestSpec { id="set_10x",      title="Rotate to 10× objective",    kind=GoalKind.SetTurret, objective=10 },
            new QuestSpec { id="set_40x",      title="Rotate to 40× objective",    kind=GoalKind.SetTurret, objective=40 },
            new QuestSpec { id="set_100x",     title="Rotate to 100× objective",   kind=GoalKind.SetTurret, objective=100 },
        };

        private IEnumerable<QuestSpec> GetActiveCatalog()
        {
            if (!useBuiltInDefaultCatalog && questCatalog != null && questCatalog.steps != null && questCatalog.steps.Count > 0)
                return questCatalog.steps;

            return DefaultCatalog;
        }

        // ===== Runtime =====
        private readonly Queue<(QuestGoal goal, string id, GoalParams parameters)> goalQueue = new();
        private (QuestGoal goal, string id, GoalParams parameters)? currentGoalInfo;
        private int runStamp;

        private void Awake()
        {
            if (logProgress) Debug.LogWarning($"[Quest] Runner AWAKE on '{gameObject.name}'");
        }

        private void OnEnable()
        {
            if (logProgress) Debug.LogWarning($"[Quest] Runner ENABLED (autoStart={autoStart})");
        }

        private void Start()
        {
            if (logProgress) Debug.LogWarning("[Quest] Runner START()");
            if (autoStart) BuildAndStartQuest();
        }

        // ================== Build & Start (for-loop katalog) ==================
        public void BuildAndStartQuest()
        {
            if (logProgress) Debug.LogWarning("[Quest] BuildAndStartQuest() CALLED");

            runStamp++;
            StopAllCoroutines();

            if (currentGoalInfo is { } cg)
            {
                cg.goal.Cancel();
                currentGoalInfo = null;
            }

            goalQueue.Clear();

            // LOOP katalog → instantiate goal + parameters berdasarkan jenisnya
            foreach (var spec in GetActiveCatalog())
            {
                switch (spec.kind)
                {
                    case GoalKind.AssembleSlide:
                    {
                        var param = new Dictionary<string, object>
                        {
                            { "require_slice", spec.requireSlice },
                            { "require_water_drop", spec.requireWaterDrop },
                            { "require_cover_slip", spec.requireCoverSlip }
                        };
                        goalQueue.Enqueue((new AssembleSlideGoal(), spec.id, BuildParameters(param)));
                        break;
                    }

                    case GoalKind.PlaceSlide:
                    {
                        goalQueue.Enqueue((new PlaceSlideGoal(), spec.id, BuildParameters(null)));
                        break;
                    }

                    case GoalKind.SetTurret:
                    {
                        var param = new Dictionary<string, object> { { "objective", spec.objective } };
                        goalQueue.Enqueue((new SetTurretGoal(), spec.id, BuildParameters(param)));
                        break;
                    }
                }
            }

            if (logProgress) Debug.LogWarning($"[Quest] Built {goalQueue.Count} steps.");

            OnQuestBuilt?.Invoke();

            AdvanceToNextGoal();
        }

        private static GoalParams BuildParameters(Dictionary<string, object> map) =>
            new GoalParams(map ?? new());

        private void AdvanceToNextGoal()
        {
            if (currentGoalInfo is { } current)
            {
                current.goal.Cancel();
                currentGoalInfo = null;
            }

            // Jika antrean habis → selesai
            if (goalQueue.Count == 0)
            {
                if (logProgress) Debug.LogWarning("[Quest] ALL DONE — Microscope quest completed!");
                OnQuestCompleted?.Invoke(); // UI hook
                return;
            }

            // Ambil item berikutnya
            var nextItem = goalQueue.Dequeue();
            currentGoalInfo = nextItem;

            if (logProgress)
                Debug.LogWarning($"[Quest] START  → {nextItem.id} ({nextItem.goal.GetType().Name})");

            // UI: step mulai
            OnStepStarted?.Invoke(nextItem.id);

            // Mulai dan tunggu sampai selesai
            nextItem.goal.Begin(nextItem.id, nextItem.parameters);
            StartCoroutine(WaitUntilGoalFinished(nextItem.goal, nextItem.id));
        }

        /// <summary>Tunggu sampai goal selesai/failed, lalu lanjut.</summary>
        private System.Collections.IEnumerator WaitUntilGoalFinished(QuestGoal goal, string goalId)
        {
            while (goal.State == GoalState.Active) yield return null;

            if (goal.State == GoalState.Completed)
            {
                if (logProgress) Debug.LogWarning($"[Quest] DONE   ✓ {goalId}");
                OnStepFinished?.Invoke(goalId, true);
            }
            else if (goal.State == GoalState.Failed)
            {
                if (logProgress) Debug.LogWarning($"[Quest] FAILED ✗ {goalId}");
                OnStepFinished?.Invoke(goalId, false);
            }
            else
            {
                // State lain (Cancelled/Moot) dianggap belum selesai
                OnStepFinished?.Invoke(goalId, false);
            }

            AdvanceToNextGoal();
        }

        // ================== (Opsional) bantuan untuk UI ==================
        // Supaya UI bisa render list langkah tanpa data baru.
        public List<(string id, string title)> GetPlannedSteps()
        {
            var list = new List<(string id, string title)>();
            foreach (var s in GetActiveCatalog())
                list.Add((s.id, s.title));
            return list;
        }
    }
}
