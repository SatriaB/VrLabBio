using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    public abstract class QuestRunner : MonoBehaviour
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
        protected readonly Queue<(QuestGoal goal, string id, GoalParams parameters)> goalQueue = new();
        protected (QuestGoal goal, string id, GoalParams parameters)? currentGoalInfo;

        protected virtual void Start()
        {
            if (logProgress) Debug.LogWarning("[Quest] Runner START()");
            if (autoStart) BuildAndStartQuest();  // sama seperti di runner kamu sekarang. :contentReference[oaicite:3]{index=3}
        }

        public void BuildAndStartQuest()
        {
            if (logProgress) Debug.LogWarning("[Quest] BuildAndStartQuest() CALLED");  // :contentReference[oaicite:4]{index=4}

            StopAllCoroutines();

            if (currentGoalInfo is { } cg)
            {
                cg.goal.Cancel();
                currentGoalInfo = null;
            }

            goalQueue.Clear();

            foreach (var spec in GetActiveCatalog())
            {
                var built = BuildGoal(spec);
                if (built.goal == null)
                {
                    Debug.LogWarning($"[Quest] Skip spec '{spec.id}' (goal null)");
                    continue;
                }
                goalQueue.Enqueue(built);
            }

            if (logProgress) Debug.LogWarning($"[Quest] Built {goalQueue.Count} steps."); // :contentReference[oaicite:5]{index=5}
            OnQuestBuilt?.Invoke();
            AdvanceToNextGoal();
        }

        protected abstract IEnumerable<QuestSpec> GetActiveCatalog();
        protected abstract (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec);

        protected static GoalParams BuildParameters(Dictionary<string, object> map) =>
            new GoalParams(map ?? new()); // sama seperti di file kamu. :contentReference[oaicite:6]{index=6}

        protected void AdvanceToNextGoal()
        {
            if (currentGoalInfo is { } current)
            {
                current.goal.Cancel();
                currentGoalInfo = null;
            }

            // Jika antrean habis → selesai
            if (goalQueue.Count == 0)
            {
                if (logProgress) Debug.LogWarning("[Quest] ALL DONE — quest completed!");
                OnQuestCompleted?.Invoke();
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

        private System.Collections.IEnumerator WaitUntilGoalFinished(QuestGoal goal, string goalId)
        {
            while (goal.State == GoalState.Active) yield return null; // sama. :contentReference[oaicite:9]{index=9}

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
                OnStepFinished?.Invoke(goalId, false);
            }

            AdvanceToNextGoal();
        }
        
        public List<(string id, string title)> GetPlannedSteps()
        {
            var list = new List<(string id, string title)>();
            foreach (var spec in GetActiveCatalog())
            {
                list.Add((spec.id, spec.title));
            }
            return list;
        }
    }
}
