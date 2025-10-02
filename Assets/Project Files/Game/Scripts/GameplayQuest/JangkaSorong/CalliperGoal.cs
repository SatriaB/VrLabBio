using UnityEngine;

namespace FatahDev
{
    public class CaliperGoal : QuestGoal
    {
        private string expectedSignal;
        private int targetIndex;

        protected override void OnBegin()
        {
            var phase = Parameters?.GetString("phase", "level") ?? "level";
            expectedSignal = PhaseToSignal(phase, out targetIndex);
            
            QuestEvents.Subscribe(expectedSignal, OnEvent);
        }

        protected override void OnCancel()   => Unsub();
        protected override void OnComplete() { Unsub(); }

        private void Unsub()
        {
            if (!string.IsNullOrEmpty(expectedSignal))
                QuestEvents.Unsubscribe(expectedSignal, OnEvent);
        }

        private void OnEvent(QuestEvents.QuestEventData e)
        {
            VRLWorks.CompleteCaliper(targetIndex, e.Name);
            Complete();
        }

        private static string PhaseToSignal(string p, out int index)
        {
            switch (p)
            {
                case "Place":   
                    index = 1;
                    return QuestSignals.CALIPER_PLACED;
                case "Contact": 
                    index = 2;
                    return QuestSignals.CALIPER_SPECIMEN_PLACED;
                //case Phase.Capture: return QuestSignals.CALIPER_MEASURE_CAPTURED;
            }

            index = 0;
            return "";
        }
    }
}