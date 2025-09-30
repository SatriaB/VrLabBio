namespace FatahDev
{
    // Param (GoalParams): "phase" = "zero" | "place" | "contact" | "lock" | "capture"
    public class MicrometerGoal : QuestGoal
    {
        private string expectedSignal;

        protected override void OnBegin()
        {
            var phase = (Parameters?.GetString("phase", "zero") ?? "zero").ToLowerInvariant();
            expectedSignal = PhaseToSignal(phase);

            if (string.IsNullOrEmpty(expectedSignal))
            {
                UnityEngine.Debug.LogWarning($"[MicrometerGoal] phase '{phase}' tidak valid. Fallback ke 'zero'.");
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

        private void OnEvent(QuestEvents.QuestEventData e)
        {
            Complete();
        }

        private static string PhaseToSignal(string phase)
        {
            switch (phase)
            {
                case "place":   return QuestSignals.MICROMETER_SPECIMEN_PLACED;
                case "capture": return QuestSignals.MICROMETER_MEASURE_CAPTURED;
                default:        return null;
            }
        }
    }
}