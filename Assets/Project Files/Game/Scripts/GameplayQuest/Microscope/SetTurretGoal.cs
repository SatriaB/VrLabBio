namespace FatahDev
{
    // Menunggu objektif tertentu (4/10/40/100). Selesai saat event sesuai ter-emit.
    // Parameter: "objective" (int) — default 4 jika tidak diisi.
    public class SetTurretGoal : QuestGoal
    {
        private int targetMagnification;
        private int targetIndex;
        private string targetSignal;

        protected override void OnBegin()
        {
            // baca param dari runner/catalog
            targetMagnification = Parameters?.GetInt("objective", 4) ?? 4;
            targetSignal = MapSignal(targetMagnification, out int index);
            targetIndex = index;

            // fallback aman kalau angka tidak valid
            if (string.IsNullOrEmpty(targetSignal))
            {
                targetMagnification = 4;
                targetSignal = QuestSignals.OBJECTIVE_SET_4X;
                UnityEngine.Debug.LogWarning($"[SetTurretGoal] objective tidak valid. Fallback ke 4x.");
            }

            QuestEvents.Subscribe(targetSignal, OnTurretEvent);
        }

        protected override void OnCancel()   => UnsubscribeSafe();
        protected override void OnComplete() => UnsubscribeSafe();

        private void UnsubscribeSafe()
        {
            if (!string.IsNullOrEmpty(targetSignal))
                QuestEvents.Unsubscribe(targetSignal, OnTurretEvent);
        }

        // Signature harus (QuestEvents.QuestEventData e)
        private void OnTurretEvent(QuestEvents.QuestEventData e)
        {
            // Karena kita subscribe ke event-name spesifik, cukup Complete().
            VRLWorks.CompleteMicroscope(targetIndex, e.Name);
            Complete();
        }

        private static string MapSignal(int magnification, out int index)
        {
            switch (magnification)
            {
                case 4:
                    index = 35;
                    return QuestSignals.OBJECTIVE_SET_4X;
                case 10:  
                    index = 40;
                    return QuestSignals.OBJECTIVE_SET_10X;
                case 40: 
                    index = 45;
                    return QuestSignals.OBJECTIVE_SET_40X;
                case 100: 
                    index = 51;
                    return QuestSignals.OBJECTIVE_SET_100X;
                default:  
                    index = 35;
                    return null;
            }
        }
    }
}