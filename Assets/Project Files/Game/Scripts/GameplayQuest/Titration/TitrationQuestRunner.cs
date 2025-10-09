using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class TitrationQuestRunner : QuestRunner
    {
        private static readonly QuestSpec[] DefaultCatalog =
        {
            new() { id="tit_wash",   title="Bilas buret dengan akuades, kemudian bilas kembali dengan larutan natrium hidroksida (NaOH) 1M yang akan digunakan",  kind=GoalKind.Titration, phase="Place"},
            new() { id="tit_fill",   title="Isi buret dengan larutan NaOH 1M sebanyak 100 ml",        kind=GoalKind.Titration, phase="Contact"},
            new() { id="tit_pour", title="Ambilah larutan asam klorida (10 ml) yang telah ditetesi 3 tetes fenolftalin yang belum diketahui konsentrasinya ke dalam erlenmeyer",    kind=GoalKind.Titration, phase="capture"},
            new() { id="tit_tit", title="Titrasi larutan dalam Erlenmeyer dengan larutan NaOH di dalam buret hingga terjadi perubahan warna",    kind=GoalKind.Titration, phase="capture"},
            new() { id="tit_stop", title="Hentikan titrasi begitu terjadi perubahan warna",    kind=GoalKind.Titration, phase="capture"},
            new() { id="tit_write", title="Catat volume NaOH yang terpakai",    kind=GoalKind.Titration, phase="capture"},
            new() { id="tit_repeat", title="Lakukan titrasi sebanyak 3 kali",    kind=GoalKind.Titration, phase="capture"}
            //new() { id="cal_capture", title="Capture hasil ukur",    kind=GoalKind.Caliper, phase="capture"},
        };

        protected override IEnumerable<QuestSpec> GetActiveCatalog() => DefaultCatalog;

        protected override (QuestGoal goal, string id, GoalParams parameters) BuildGoal(QuestSpec spec)
        {
            if (spec.kind == GoalKind.Titration)
            {
                return (new TitrationGoal(),
                    spec.id,
                    BuildParameters(new Dictionary<string, object> { { "phase", spec.phase ?? "all" } }));
            }

            Debug.LogWarning($"[TitrationQuestRunner] Unsupported kind in this runner: {spec.kind}");
            return (null, spec.id, null);
        }
    }
}