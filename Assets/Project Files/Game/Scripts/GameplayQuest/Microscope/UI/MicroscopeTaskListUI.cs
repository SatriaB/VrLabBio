using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class MicroscopeTaskListUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform contentRoot;   
        [SerializeField] private GameObject itemPrefab;   
        [SerializeField] private MicroscopeQuestRunner runner;

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

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            List<(string id, string title)> planned = null;
            planned = runner != null ? runner.GetPlannedSteps() : new List<(string id, string title)>();

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

        public void HandleQuestBuilt()
        {
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
