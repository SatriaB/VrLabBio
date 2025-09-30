namespace FatahDev
{
    public class CaliperGoal : QuestGoal
    {
        public enum Phase { Zero, Place, Contact, Capture }
        public Phase phase = Phase.Zero;

        private string expectedSignal;

        protected override void OnBegin()
        {
            expectedSignal = PhaseToSignal(phase);
            QuestEvents.Subscribe(expectedSignal, OnEvent);
        }

        protected override void OnCancel()   => Unsub();
        protected override void OnComplete() { Unsub(); }

        private void Unsub()
        {
            if (!string.IsNullOrEmpty(expectedSignal))
                QuestEvents.Unsubscribe(expectedSignal, OnEvent);
        }

        private void OnEvent(QuestEvents.QuestEventData e) => Complete();

        private static string PhaseToSignal(Phase p)
        {
            switch (p)
            {
                case Phase.Place:   return QuestSignals.CALIPER_SPECIMEN_PLACED;
                case Phase.Contact: return QuestSignals.CALIPER_CONTACT_OK;
                case Phase.Capture: return QuestSignals.CALIPER_MEASURE_CAPTURED;
            }

            return "";
        }
    }
}