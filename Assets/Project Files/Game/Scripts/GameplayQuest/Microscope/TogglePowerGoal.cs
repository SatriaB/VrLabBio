namespace FatahDev
{
    public class TogglePowerGoal : QuestGoal
    {
        private bool waitForOn = true;

        protected override void OnBegin()
        {
            if (Parameters != null)
                waitForOn = Parameters.GetBool("value", true);

            if (!waitForOn)
            {
                Complete();
                return;
            }

            QuestEvents.Subscribe(QuestSignals.MICROSCOPE_ON, OnPowerEvent);
        }

        protected override void OnCancel()   => Unsub();
        protected override void OnComplete() => Unsub();

        private void Unsub()
        {
            QuestEvents.Unsubscribe(QuestSignals.MICROSCOPE_ON, OnPowerEvent);
        }

        // Signature WAJIB: (QuestEvents.QuestEventData e)
        private void OnPowerEvent(QuestEvents.QuestEventData e)
        {
            if (e.Name == QuestSignals.MICROSCOPE_ON)
            {
                VRLWorks.CompleteMicroscope(28, e.Name);
                Complete();
            }
        }
    }
}