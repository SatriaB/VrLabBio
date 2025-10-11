namespace FatahDev
{
    public class AssembleSlideGoal : QuestGoal
    {
        private bool samplePicked, samplePlaced, waterDropped, slideInserted;
        private bool singlePhaseMode; // true kalau pakai 'phase' (pick/place/water/insert)
        private string expectedSignal; // sinyal tunggal yang ditunggu pada single-phase

        protected override void OnBegin()
        {
            samplePicked = samplePlaced = waterDropped = slideInserted = false;

            // Baca parameter 'phase' (default: "all")
            var phase = Parameters?.GetString("phase", "all") ?? "all";
            phase = phase.ToLowerInvariant();

            singlePhaseMode = phase != "all";
            expectedSignal  = MapPhaseToSignal(phase);

            if (singlePhaseMode)
            {
                // tunggu SATU sinyal saja
                if (string.IsNullOrEmpty(expectedSignal))
                {
                    UnityEngine.Debug.LogWarning($"[AssembleSlideGoal] phase '{phase}' tidak valid. Pakai 'all'.");
                    singlePhaseMode = false;
                    SubscribeAll();
                }
                else
                {
                    QuestEvents.Subscribe(expectedSignal, OnEvent);
                }
            }
            else
            {
                // gabungan: tunggu keempat sinyal
                SubscribeAll();
            }
        }

        protected override void OnCancel()   => UnsubAll();
        protected override void OnComplete() => UnsubAll();

        private void SubscribeAll()
        {
            QuestEvents.Subscribe(QuestSignals.PINSET_SAMPLE_PICKED,   OnEvent);
            QuestEvents.Subscribe(QuestSignals.SAMPLE_PLACED_ON_SLIDE, OnEvent);
            QuestEvents.Subscribe(QuestSignals.WATER_DROPPED_ON_SLIDE, OnEvent);
            QuestEvents.Subscribe(QuestSignals.SLIDE_INSERTED,         OnEvent);
        }

        private void UnsubAll()
        {
            // aman walau belum pernah di-subscribe
            QuestEvents.Unsubscribe(QuestSignals.PINSET_SAMPLE_PICKED,   OnEvent);
            QuestEvents.Unsubscribe(QuestSignals.SAMPLE_PLACED_ON_SLIDE, OnEvent);
            QuestEvents.Unsubscribe(QuestSignals.WATER_DROPPED_ON_SLIDE, OnEvent);
            QuestEvents.Unsubscribe(QuestSignals.SLIDE_INSERTED,         OnEvent);

            if (!string.IsNullOrEmpty(expectedSignal))
                QuestEvents.Unsubscribe(expectedSignal, OnEvent);
        }

        // HARUS cocok sama QuestEvents.cs kamu
        private void OnEvent(QuestEvents.QuestEventData e)
        {
            if (singlePhaseMode)
            {
                // cukup sekali event yang sesuai → complete
                if (e.Name == expectedSignal)
                    Complete();
                return;
            }

            // mode gabungan (all)
            if (e.Name == QuestSignals.PINSET_SAMPLE_PICKED)
            {
                VRLWorks.CompleteMicroscope(23, e.Name);
                samplePicked  = true;
            }
            else if (e.Name == QuestSignals.SAMPLE_PLACED_ON_SLIDE)
            {
                VRLWorks.CompleteMicroscope(24, e.Name);
                samplePlaced  = true;
            }
            else if (e.Name == QuestSignals.WATER_DROPPED_ON_SLIDE)
            {
                VRLWorks.CompleteMicroscope(25, e.Name);
                waterDropped  = true;
            }
            else if (e.Name == QuestSignals.SLIDE_INSERTED)
            {
                VRLWorks.CompleteMicroscope(31, e.Name);
                slideInserted = true;
            }

            if (samplePicked && samplePlaced && waterDropped && slideInserted)
                Complete();
        }

        private static string MapPhaseToSignal(string phase)
        {
            switch (phase)
            {
                case "pick":   return QuestSignals.PINSET_SAMPLE_PICKED;
                case "place":  return QuestSignals.SAMPLE_PLACED_ON_SLIDE;
                case "water":  return QuestSignals.WATER_DROPPED_ON_SLIDE;
                case "insert": return QuestSignals.SLIDE_INSERTED;
                case "all":    return null; // ditangani sebagai multi-subscribe
                default:       return null;
            }
        }
    }
}
