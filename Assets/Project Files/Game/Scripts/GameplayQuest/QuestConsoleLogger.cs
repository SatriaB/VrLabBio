using UnityEngine;

namespace FatahDev
{
    public class QuestConsoleLogger : MonoBehaviour
    {
        void OnEnable()
        {
            QuestEvents.Subscribe("quest.goal.started", Log);
            QuestEvents.Subscribe("quest.goal.completed", Log);
            QuestEvents.Subscribe("quest.goal.failed", Log);
            QuestEvents.Subscribe("quest.run.completed", Log);
        }

        void OnDisable()
        {
            QuestEvents.Unsubscribe("quest.goal.started", Log);
            QuestEvents.Unsubscribe("quest.goal.completed", Log);
            QuestEvents.Unsubscribe("quest.goal.failed", Log);
            QuestEvents.Unsubscribe("quest.run.completed", Log);
        }

        void Log(QuestEvents.QuestEventData e) => Debug.Log($"[Quest] {e.Name} :: {e.Payload}");
    }
}