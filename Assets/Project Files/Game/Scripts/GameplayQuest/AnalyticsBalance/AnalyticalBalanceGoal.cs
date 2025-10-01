namespace FatahDev
{
    public class AnalyticalBalanceGoal : QuestGoal
    {
        private string expectedSignal;
        private int stepId;

        protected override void OnBegin()
        {
            var phase = (Parameters?.GetString("phase", "level") ?? "level").ToLowerInvariant();
            expectedSignal = PhaseToSignal(phase, out stepId);

            if (string.IsNullOrEmpty(expectedSignal))
            {
                UnityEngine.Debug.LogWarning($"[AnalyticalBalanceGoal] phase '{phase}' tidak valid. Fallback 'level'.");
            }

            QuestEvents.Subscribe(expectedSignal, OnEvent);
        }

        protected override void OnCancel()   => Unsub();
        protected override void OnComplete() => Unsub();

        private void Unsub()
        {
            if (!string.IsNullOrEmpty(expectedSignal))
                QuestEvents.Unsubscribe(expectedSignal, OnEvent);
        }

        // signature: (QuestEvents.QuestEventData e)
        private void OnEvent(QuestEvents.QuestEventData e)
        {
            VRLWorks.CompleteAnalyticalBalance(stepId, e.Name);
            Complete();
        }

        private static string PhaseToSignal(string phase, out int stepId)
        {
            switch (phase)
            {
                case "place_container":
                    stepId = 0;
                    return QuestSignals.BALANCE_CONTAINER_PLACED;
                case "place_sample":    
                    stepId = 1;
                    return QuestSignals.BALANCE_SAMPLE_PLACED;
                /*case "stable":          return QuestSignals.BALANCE_STABLE_READING;
                case "capture":         return QuestSignals.BALANCE_CAPTURED;
                case "clean":           return QuestSignals.BALANCE_CLEANED;*/
                default:                
                    stepId = 0;
                    return null;
            }
        }
    }
}
