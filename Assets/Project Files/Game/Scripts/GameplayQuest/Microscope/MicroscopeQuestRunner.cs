using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    /// <summary>
    /// Menjalankan urutan quest mikroskop: Persiapan → 4× → 10× → 40× → 100× (minyak) → Shutdown.
    /// Tambahan: UI hooks (UnityEvent) untuk mengikat ke UI checklist (DONE/belum).
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

        /// <summary>Build antrean step dan mulai eksekusi.</summary>
        public void BuildAndStartQuest()
        {
            if (logProgress) Debug.LogWarning("[Quest] BuildAndStartQuest() CALLED");

            runStamp++;
            StopAllCoroutines();

            // Batalkan goal berjalan (bersihin subscribe/OnCancel)
            if (currentGoalInfo is { } cg)
            {
                cg.goal.Cancel();
                currentGoalInfo = null;
            }

            goalQueue.Clear();

            // ================== PHASE 0 — Persiapan preparat (opsional) ==================
            goalQueue.Enqueue((new AssembleSlideGoal(), "prep_slide", BuildParameters(new()
            {
                { "require_slice", true }, { "require_water_drop", true }, { "require_cover_slip", true }
            })));

            // ================== PHASE 1 — Power ON + set 4× ==================
            goalQueue.Enqueue((new TogglePowerGoal(), "power_on", BuildParameters(new() { { "value", true } })));
            goalQueue.Enqueue((new SetTurretGoal(), "set_4x", BuildParameters(new() { { "objective", 4 } })));

            // ================== PHASE 2 — 4× ==================
            goalQueue.Enqueue((new PlaceSlideGoal(), "place_slide", BuildParameters(null)));
            goalQueue.Enqueue((new AchieveFocusGoal(), "focus_4x", BuildParameters(new()
            {
                { "objective", 4 }, { "order_macro_then_micro", true }, { "tolerance", 0.95f }
            })));
            goalQueue.Enqueue((new CaptureImageGoal(), "capture_4x", BuildParameters(new() { { "objective", 4 } })));

            // ================== PHASE 3 — 10× ==================
            goalQueue.Enqueue((new SetTurretGoal(), "set_10x", BuildParameters(new() { { "objective", 10 } })));
            goalQueue.Enqueue((new AchieveFocusGoal(), "focus_10x", BuildParameters(new()
            {
                { "objective", 10 }, { "tolerance", 0.95f }
            })));
            goalQueue.Enqueue((new CaptureImageGoal(), "capture_10x", BuildParameters(new() { { "objective", 10 } })));

            // ================== PHASE 4 — 40× ==================
            goalQueue.Enqueue((new SetTurretGoal(), "set_40x", BuildParameters(new() { { "objective", 40 } })));
            goalQueue.Enqueue((new AchieveFocusGoal(), "focus_40x", BuildParameters(new()
            {
                { "objective", 40 }, { "tolerance", 0.95f }
            })));
            goalQueue.Enqueue((new CaptureImageGoal(), "capture_40x", BuildParameters(new() { { "objective", 40 } })));

            // ================== PHASE 5 — 100× (dengan minyak) ==================
            goalQueue.Enqueue((new ApplyOilGoal(), "apply_oil", BuildParameters(null)));
            goalQueue.Enqueue((new SetTurretGoal(), "set_100x", BuildParameters(new() { { "objective", 100 } })));
            goalQueue.Enqueue((new AchieveFocusGoal(), "focus_100x", BuildParameters(new()
            {
                { "objective", 100 }, { "tolerance", 0.95f }
            })));
            goalQueue.Enqueue((new CaptureImageGoal(), "capture_100x", BuildParameters(new() { { "objective", 100 } })));

            // ================== PHASE 6 — Shutdown & Perawatan ==================
            goalQueue.Enqueue((new RaiseObjectiveGoal(), "raise_objective_safe", BuildParameters(new()
            {
                { "safe_distance_mm", 10 }
            })));
            goalQueue.Enqueue((new CleanLensGoal(), "clean_lens", BuildParameters(null)));
            goalQueue.Enqueue((new SetTurretGoal(), "back_to_4x", BuildParameters(new() { { "objective", 4 } })));
            goalQueue.Enqueue((new TogglePowerGoal(), "power_off", BuildParameters(new() { { "value", false } })));
            goalQueue.Enqueue((new DockMicroscopeGoal(), "dock", BuildParameters(new()
            {
                { "require_two_handed_pickup", true }, { "require_correct_orientation", true }
            })));

            if (logProgress) Debug.LogWarning($"[Quest] Built {goalQueue.Count} steps.");

            // Beri tahu UI untuk rebuild list (DONE/belum)
            OnQuestBuilt?.Invoke();

            AdvanceToNextGoal();
        }

        private static GoalParams BuildParameters(Dictionary<string, object> map) =>
            new GoalParams(map ?? new());

        /// <summary>Melanjutkan ke goal berikutnya dalam antrean.</summary>
        private void AdvanceToNextGoal()
        {
            // Hentikan goal aktif (jika ada) sebelum lanjut
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
    }
}
