namespace FatahDev
{
    using static QuestEvents;
    using static QuestSignals;
    
    public static class MicroscopeEventNames
    {
        //public const string SlidePrepared   = "slide.prepared";
        public const string SliceDone       = "slide.slice.done";
        public const string WaterDropped    = "slide.water.dropped";
        //public const string CoverApplied    = "slide.cover.applied";
    }


    public class AssembleSlideGoal : QuestGoal
    {
        bool requireSlice, requireWaterDrop, requireCoverSlip;
        bool sliceCompleted, waterDropped, coverApplied;
        bool completedOnce;

        protected override void OnBegin()
        {
            // reset state lokal
            completedOnce = false;
            sliceCompleted = false;
            waterDropped = false;
            coverApplied = false;

            // baca parameter
            requireSlice = Parameters.GetBool("require_slice", true);
            requireWaterDrop = Parameters.GetBool("require_water_drop", true);
            requireCoverSlip = Parameters.GetBool("require_cover_slip", true);

            // subscribe event agregat + atomik
            //Subscribe(MicroscopeEventNames.SlidePrepared, OnAnySlideEvent);
            Subscribe(MicroscopeEventNames.SliceDone, OnAnySlideEvent);
            Subscribe(MicroscopeEventNames.WaterDropped, OnAnySlideEvent);
            //Subscribe(MicroscopeEventNames.CoverApplied, OnAnySlideEvent);

            // kalau semua requirement false (atau sudah satisfied dari luar), lulus instan
            TryComplete();
        }

        void OnAnySlideEvent(QuestEventData e)
        {
            if (completedOnce) return; // guard idempotent

            // agregat (boleh langsung lulus)
            /*if (e.Name == MicroscopeEventNames.SlidePrepared)
            {
                CompleteSafe();
                return;
            }*/

            // atomik
            if (e.Name == MicroscopeEventNames.SliceDone) sliceCompleted = true;
            if (e.Name == MicroscopeEventNames.WaterDropped) waterDropped = true;
            //if (e.Name == MicroscopeEventNames.CoverApplied) coverApplied = true;

            TryComplete();
        }

        void TryComplete()
        {
            if (completedOnce) return;

            bool okSlice = !requireSlice || sliceCompleted;
            bool okWater = !requireWaterDrop || waterDropped;
            bool okCover = !requireCoverSlip || coverApplied;

            if (okSlice && okWater && okCover)
                CompleteSafe();
        }

        void CompleteSafe()
        {
            if (completedOnce) return;
            completedOnce = true;
            Complete(); // panggil API milik base class kamu
        }

        protected override void OnCancel()
        {
            UnsubAll();
        }

        // penting: lepas listener saat selesai juga
        protected override void OnComplete()
        {
            UnsubAll();
        }

        void UnsubAll()
        {
           // QuestEvents.Unsubscribe(MicroscopeEventNames.SlidePrepared, OnAnySlideEvent);
            QuestEvents.Unsubscribe(MicroscopeEventNames.SliceDone, OnAnySlideEvent);
            QuestEvents.Unsubscribe(MicroscopeEventNames.WaterDropped, OnAnySlideEvent);
            //QuestEvents.Unsubscribe(MicroscopeEventNames.CoverApplied, OnAnySlideEvent);
        }
    }

    // 1) TogglePowerGoal
    public class TogglePowerGoal : QuestGoal
    {
        bool targetPowerOn;
        protected override void OnBegin()
        {
            targetPowerOn = Parameters.GetBool("value", true);
            QuestEvents.Subscribe(targetPowerOn ? PowerOn : PowerOff, OnPowerChanged);
        }
        void OnPowerChanged(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(targetPowerOn ? PowerOn : PowerOff, OnPowerChanged);
        }
    }

    // 2) SetTurretGoal
    public class SetTurretGoal : QuestGoal
    {
        int objectiveMagnification;
        protected override void OnBegin()
        {
            objectiveMagnification = Parameters.GetInt("objective", 4);
            QuestEvents.Subscribe(QuestSignals.TurretSet(objectiveMagnification), OnTurretSet);
        }
        void OnTurretSet(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(QuestSignals.TurretSet(objectiveMagnification), OnTurretSet);
        }
    }

    // 3) PlaceSlideGoal
    public class PlaceSlideGoal : QuestGoal
    {
        protected override void OnBegin()
        {
            QuestEvents.Subscribe(SlideOnStage, OnSlidePlacedOnStage);
        }
        void OnSlidePlacedOnStage(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(SlideOnStage, OnSlidePlacedOnStage);
        }
    }

    // 4) AchieveFocusGoal
    public class AchieveFocusGoal : QuestGoal
    {
        int objectiveMagnification;
        float qualityTolerance;
        bool requireMacroThenMicroOrder;

        protected override void OnBegin()
        {
            objectiveMagnification    = Parameters.GetInt("objective", 4);
            qualityTolerance          = Parameters.GetFloat("tolerance", 0.95f);
            requireMacroThenMicroOrder= Parameters.GetBool("order_macro_then_micro", false);
            QuestEvents.Subscribe(QuestSignals.FocusOk(objectiveMagnification), OnFocusAchieved);
        }

        void OnFocusAchieved(QuestEventData eventData)
        {
            if (eventData.Payload is FocusPayload focusPayload)
            {
                if (focusPayload.objective != objectiveMagnification) return;
                if (focusPayload.quality < qualityTolerance) return;
                if (requireMacroThenMicroOrder && !focusPayload.usedMacroThenMicro) return;
            }
            Complete();
        }

        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(QuestSignals.FocusOk(objectiveMagnification), OnFocusAchieved);
        }
    }

    // 5) CaptureImageGoal
    public class CaptureImageGoal : QuestGoal
    {
        int objectiveMagnification;
        protected override void OnBegin()
        {
            objectiveMagnification = Parameters.GetInt("objective", 4);
            QuestEvents.Subscribe(QuestSignals.CaptureSaved(objectiveMagnification), OnImageCaptured);
        }
        void OnImageCaptured(QuestEventData eventData)
        {
            // Optionally validate payload path/id here
            Complete();
        }
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(QuestSignals.CaptureSaved(objectiveMagnification), OnImageCaptured);
        }
    }

    // 6) ApplyOilGoal
    public class ApplyOilGoal : QuestGoal
    {
        protected override void OnBegin()
        {
            QuestEvents.Subscribe(OilApplied, OnOilApplied);
        }
        void OnOilApplied(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(OilApplied, OnOilApplied);
        }
    }

    // 7) RaiseObjectiveGoal
    public class RaiseObjectiveGoal : QuestGoal
    {
        protected override void OnBegin()
        {
            QuestEvents.Subscribe(ObjectiveSafe, OnObjectiveRaisedToSafeDistance);
        }
        void OnObjectiveRaisedToSafeDistance(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(ObjectiveSafe, OnObjectiveRaisedToSafeDistance);
        }
    }

    // 8) CleanLensGoal
    public class CleanLensGoal : QuestGoal
    {
        protected override void OnBegin()
        {
            QuestEvents.Subscribe(LensCleaned, OnLensCleaned);
        }
        void OnLensCleaned(QuestEventData eventData) => Complete();
        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(LensCleaned, OnLensCleaned);
        }
    }

    // 9) DockMicroscopeGoal
    public class DockMicroscopeGoal : QuestGoal
    {
        bool requireTwoHandedPickup, requireCorrectOrientation;
        bool twoHandedPickupVerified, correctOrientationVerified, microscopeDocked;

        protected override void OnBegin()
        {
            requireTwoHandedPickup   = Parameters.GetBool("require_two_handed_pickup", true);
            requireCorrectOrientation= Parameters.GetBool("require_correct_orientation", true);

            QuestEvents.Subscribe(MicrosDocked, OnMicroscopeDocked);
            QuestEvents.Subscribe("pickup.two_handed", OnTwoHandedPickupConfirmed);
            QuestEvents.Subscribe("dock.orientation.ok", OnDockOrientationConfirmed);
        }

        void OnTwoHandedPickupConfirmed(QuestEventData eventData) => twoHandedPickupVerified = true;
        void OnDockOrientationConfirmed(QuestEventData eventData) => correctOrientationVerified = true;

        void OnMicroscopeDocked(QuestEventData eventData)
        {
            microscopeDocked = true;

            if (eventData.Payload is DockedPayload dockedPayload)
            {
                twoHandedPickupVerified = dockedPayload.twoHandedPickup || !requireTwoHandedPickup;
                correctOrientationVerified = dockedPayload.orientationOk || !requireCorrectOrientation;
            }

            if ((!requireTwoHandedPickup || twoHandedPickupVerified) &&
                (!requireCorrectOrientation || correctOrientationVerified) &&
                microscopeDocked)
            {
                Complete();
            }
        }

        protected override void OnCancel()
        {
            QuestEvents.Unsubscribe(MicrosDocked, OnMicroscopeDocked);
            QuestEvents.Unsubscribe("pickup.two_handed", OnTwoHandedPickupConfirmed);
            QuestEvents.Unsubscribe("dock.orientation.ok", OnDockOrientationConfirmed);
        }
    }
}
