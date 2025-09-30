using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class MicrometerQuestRunner : QuestRunner
    {
        private static readonly QuestSpec[] DefaultCatalog =
        {
            new() { id="micro_place",   title="Tempatkan Mikrometer",            kind=GoalKind.Micrometer, phase="place"},
            new() { id="micro_specimen_place", title="Tempatkan benda",          kind=GoalKind.Micrometer, phase="specimen_place" },
        };

        protected override IEnumerable<QuestSpec> GetActiveCatalog() => DefaultCatalog;

        protected override (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec)
        {
            if (spec.kind == GoalKind.Micrometer)
            {
                return (
                    new MicrometerGoal(),
                    spec.id,
                    BuildParameters(new Dictionary<string, object> { { "phase", spec.phase ?? "zero" } })
                );
            }

            Debug.LogWarning($"[MicrometerQuestRunner] Unsupported kind: {spec.kind}");
            return (null, spec.id, null);
        }
    }
}