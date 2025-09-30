using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class AnalyticalBalanceQuestRunner : QuestRunner
    {
        private static readonly QuestSpec[] DefaultCatalog =
        {
            // Penimbangan
            new()
            {
                id = "bal_placec", title = "Letakkan boat/kertas timbang", kind = GoalKind.AnalyticalBalance,
                phase = "place_container"
            },
            new()
            {
                id = "bal_places", title = "Letakkan sampel", kind = GoalKind.AnalyticalBalance, phase = "place_sample"
            },
            new()
            {
                id = "bal_capture", title = "Capture pembacaan", kind = GoalKind.AnalyticalBalance, phase = "capture"
            },
        };

        protected override IEnumerable<QuestSpec> GetActiveCatalog() => DefaultCatalog;

        protected override (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec)
        {
            if (spec.kind == GoalKind.AnalyticalBalance)
            {
                return (new AnalyticalBalanceGoal(),
                    spec.id,
                    BuildParameters(new Dictionary<string, object> { { "phase", spec.phase ?? "level" } }));
            }

            Debug.LogWarning($"[AnalyticalBalanceQuestRunner] Unsupported kind: {spec.kind}");
            return (null, spec.id, null);
        }
    }
}