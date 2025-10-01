using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class CaliperQuestRunner : QuestRunner
    {
        private static readonly QuestSpec[] DefaultCatalog =
        {
            new() { id="cal_place",   title="Letakkan Jangka Sorong",  kind=GoalKind.Caliper, phase="Place"},
            new() { id="cal_contact",   title="Letakkan benda",        kind=GoalKind.Caliper, phase="Contact"},
            //new() { id="cal_capture", title="Capture hasil ukur",    kind=GoalKind.Caliper, phase="capture"},
        };

        protected override IEnumerable<QuestSpec> GetActiveCatalog() => DefaultCatalog;

        protected override (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec)
        {
            if (spec.kind == GoalKind.Caliper)
            {
                return (new CaliperGoal(),
                    spec.id,
                    BuildParameters(new Dictionary<string, object> { { "phase", spec.phase ?? "place" } }));
            }

            Debug.LogWarning($"[CaliperQuestRunner] Unsupported kind in this runner: {spec.kind}");
            return (null, spec.id, null);
        }
    }
}