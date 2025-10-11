namespace FatahDev
{
    // Param (GoalParams): "phase" = "zero" | "place" | "contact" | "lock" | "capture"
    public class MicrometerGoal : QuestGoal
    {
        private string expectedSignal;
        private int targetIndex;

        protected override void OnBegin()
        {
            var phase = (Parameters?.GetString("phase", "zero") ?? "zero").ToLowerInvariant();
            expectedSignal = PhaseToSignal(phase, out targetIndex);

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
            VRLWorks.CompleteMicrometer(targetIndex, e.Name);
            Complete();
        }

        private static string PhaseToSignal(string phase, out int index)
        {
            switch (phase)
            {
                case "place":
                    index = 65;
                    return QuestSignals.MICROMETER_PLACED;
                case "specimen_place": 
                    index = 66;
                    return QuestSignals.MICROMETER_SPECIMEN_PLACED;
                default:        
                    index = 65;
                    return null;
            }
        }
    }
}