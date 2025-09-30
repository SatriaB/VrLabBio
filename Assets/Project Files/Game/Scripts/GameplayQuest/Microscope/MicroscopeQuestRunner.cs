using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class MicroscopeQuestRunner : QuestRunner
    {
        // tetap pakai katalog yang SAMA seperti file kamu
        private static readonly QuestSpec[] DefaultCatalog =
        {
            new() { id="prep_pick",   title="Ambil sampel (pinset)",  kind=GoalKind.AssembleSlide, phase="pick"   },
            new() { id="prep_place",  title="Taruh sampel ke slide",  kind=GoalKind.AssembleSlide, phase="place"  },
            new() { id="prep_water",  title="Teteskan air",           kind=GoalKind.AssembleSlide, phase="water"  },
            new() { id="prep_insert", title="Masukkan slide",         kind=GoalKind.AssembleSlide, phase="insert" },

            new() { id="power_on",    title="Nyalakan mikroskop",     kind=GoalKind.TogglePower },

            new() { id="obj_4x",      title="Set objektif 4×",        kind=GoalKind.SetTurret, objective=4   },
            new() { id="obj_10x",     title="Set objektif 10×",       kind=GoalKind.SetTurret, objective=10  },
            new() { id="obj_40x",     title="Set objektif 40×",       kind=GoalKind.SetTurret, objective=40  },
            new() { id="obj_100x",    title="Set objektif 100×",      kind=GoalKind.SetTurret, objective=100 },
        };

        protected override IEnumerable<QuestSpec> GetActiveCatalog()
        {
            return DefaultCatalog; 
        }

        protected override (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec)
        {
            switch (spec.kind)
            {
                case GoalKind.AssembleSlide:
                    return (new AssembleSlideGoal(),
                            spec.id,
                            BuildParameters(new Dictionary<string, object> {
                                { "phase", spec.phase ?? "all" } 
                            }));

                case GoalKind.TogglePower:
                    return (new TogglePowerGoal(),
                            spec.id,
                            BuildParameters(new Dictionary<string, object> { { "value", true } }));

                case GoalKind.SetTurret:
                    return (new SetTurretGoal(),
                            spec.id,
                            BuildParameters(new Dictionary<string, object> { { "objective", spec.objective } }));
            }

            Debug.LogWarning($"[MicroscopeQuestRunner] Unknown kind: {spec.kind}");
            return (null, spec.id, null);
        }
    }
}
