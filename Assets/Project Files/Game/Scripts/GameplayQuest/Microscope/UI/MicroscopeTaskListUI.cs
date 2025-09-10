using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    [System.Serializable]
    public struct StepDef
    {
        public string id;
        [TextArea] public string label;
    }

    /// <summary>
    /// UI checklist sederhana: hanya menandai DONE / belum.
    /// Hubungkan event dari MicroscopeQuestRunner ke metode Handle* di bawah.
    /// </summary>
    public class MicroscopeTaskListUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform contentRoot;   // parent untuk item
        [SerializeField] private GameObject itemPrefab;   // prefab berisi MicroscopeTaskItem

        // HARUS MATCH dengan ID step di MicroscopeQuestRunner kamu
        [Header("Sequence (match Runner IDs)")]
        [SerializeField] private List<StepDef> steps = new()
        {
            new StepDef{ id="prep_slide",            label="Siapkan preparat (iris + tetes air + cover)"},
            new StepDef{ id="power_on",              label="Nyalakan lampu mikroskop"},
            new StepDef{ id="set_4x",                label="Set objektif 4×"},
            new StepDef{ id="place_slide",           label="Letakkan preparat & kunci"},
            new StepDef{ id="focus_4x",              label="Fokus 4×"},
            new StepDef{ id="capture_4x",            label="Capture 4×"},

            new StepDef{ id="set_10x",               label="Set objektif 10×"},
            new StepDef{ id="focus_10x",             label="Fokus 10×"},
            new StepDef{ id="capture_10x",           label="Capture 10×"},

            new StepDef{ id="set_40x",               label="Set objektif 40×"},
            new StepDef{ id="focus_40x",             label="Fokus 40×"},
            new StepDef{ id="capture_40x",           label="Capture 40×"},

            new StepDef{ id="apply_oil",             label="Teteskan minyak imersi"},
            new StepDef{ id="set_100x",              label="Set objektif 100×"},
            new StepDef{ id="focus_100x",            label="Fokus 100×"},
            new StepDef{ id="capture_100x",          label="Capture 100×"},

            new StepDef{ id="raise_objective_safe",  label="Naikkan lensa (aman)"},
            new StepDef{ id="clean_lens",            label="Bersihkan minyak"},
            new StepDef{ id="back_to_4x",            label="Kembalikan ke 4×"},
            new StepDef{ id="power_off",             label="Matikan & cabut"},
            new StepDef{ id="dock",                  label="Simpan alat"}
        };

        private readonly Dictionary<string, MicroscopeTaskItem> items = new();

        private void Awake() => Rebuild();

        public void Rebuild()
        {
            items.Clear();
            if (!contentRoot || !itemPrefab)
            {
                Debug.LogError("[MicroscopeTaskListUI] contentRoot / itemPrefab belum diassign.");
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            foreach (var s in steps)
            {
                var go = Instantiate(itemPrefab, contentRoot, false);
                var it = go.GetComponent<MicroscopeTaskItem>();
                if (!it)
                {
                    Debug.LogError("[MicroscopeTaskListUI] itemPrefab tidak punya MicroscopeTaskItem.");
                    Destroy(go);
                    continue;
                }
                it.Setup(s.id, s.label);
                items[s.id] = it;
            }
        }

        // ==== Dipanggil dari Runner via UnityEvent ====
        public void HandleQuestBuilt() => Rebuild();

        public void HandleStepStarted(string stepId)
        {
            // (Tidak perlu apa-apa jika hanya DONE/belum. Biarkan kosong.)
        }

        public void HandleStepFinished(string stepId, bool isDone)
        {
            if (items.TryGetValue(stepId, out var it))
                it.SetDone(isDone);
        }

        public void HandleQuestCompleted()
        {
            // Opsional: tampilkan toast/ikon "Selesai".
        }
    }
}
