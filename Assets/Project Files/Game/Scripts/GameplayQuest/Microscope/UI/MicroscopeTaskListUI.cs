using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    /// <summary>
    /// UI checklist yang sepenuhnya membaca urutan langkah dari MicroscopeQuestRunner
    /// (GetPlannedSteps). Tidak ada data UI terpisah/StepDef.
    /// 
    /// Kebutuhan:
    /// - itemPrefab memiliki komponen MicroscopeTaskItem
    ///   dengan API: void Setup(string id, string label), void SetDone(bool value).
    /// - Hubungkan UnityEvent dari Runner:
    ///   OnQuestBuilt      -> HandleQuestBuilt
    ///   OnStepStarted     -> HandleStepStarted
    ///   OnStepFinished    -> HandleStepFinished
    ///   OnQuestCompleted  -> HandleQuestCompleted
    /// </summary>
    public class MicroscopeTaskListUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform contentRoot;   // parent untuk item
        [SerializeField] private GameObject itemPrefab;   // prefab berisi MicroscopeTaskItem
        [SerializeField] private MicroscopeQuestRunner runner; // assign runner di scene

        // id -> item UI
        private readonly Dictionary<string, MicroscopeTaskItem> items = new();

        private void Awake()
        {
            if (runner == null)
                Debug.LogWarning("[MicroscopeTaskListUI] Runner belum diassign. Assign di Inspector.");

            Rebuild();
        }

        [ContextMenu("Rebuild Now")]
        public void Rebuild()
        {
            items.Clear();

            if (!contentRoot || !itemPrefab)
            {
                Debug.LogError("[MicroscopeTaskListUI] contentRoot / itemPrefab belum diassign.");
                return;
            }

            // Bersihkan anak lama
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            // Ambil (id, title) langsung dari Runner
            List<(string id, string title)> planned = null;
            if (runner != null)
            {
                planned = runner.GetPlannedSteps();
            }
            else
            {
                planned = new List<(string id, string title)>();
            }

            // Bangun tiap item UI
            foreach (var (id, title) in planned)
            {
                var go = Instantiate(itemPrefab, contentRoot, false);
                var it = go.GetComponent<MicroscopeTaskItem>();
                if (!it)
                {
                    Debug.LogError("[MicroscopeTaskListUI] itemPrefab tidak memiliki MicroscopeTaskItem.");
                    Destroy(go);
                    continue;
                }

                var label = string.IsNullOrEmpty(title) ? id : title;
                it.Setup(id, label);
                items[id] = it;
            }
        }

        // ==== Dipanggil dari Runner via UnityEvent ====
        public void HandleQuestBuilt()
        {
            // Katalog bisa berubah → rebuild list.
            Rebuild();
        }

        public void HandleStepStarted(string stepId)
        {
            // Opsional: tambahkan highlight "active" di item kalau perlu.
            // Saat ini tidak melakukan apa-apa.
        }

        public void HandleStepFinished(string stepId, bool isDone)
        {
            if (items.TryGetValue(stepId, out var it))
                it.SetDone(isDone);
            // Jika item belum ada (mis. rebuild telat), bisa panggil Rebuild():
            // else Rebuild();
        }

        public void HandleQuestCompleted()
        {
            // Opsional: tampilkan toast/banner "Completed".
        }

        // ==== Util ====
        /// <summary>Update binding Runner dari script lain (opsional).</summary>
        public void SetRunner(MicroscopeQuestRunner r)
        {
            runner = r;
            Rebuild();
        }
    }
}
