﻿using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public class QuestTaskListUI : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private QuestRunner runner; // <- generik, bukan Microscope

        private readonly Dictionary<string, QuestTaskItem> items = new();
        private string lastActiveId = null;

        private void Awake()
        {
            if (runner == null)
                Debug.LogWarning("[QuestTaskListUI] Runner belum diassign. Assign di Inspector.");

            Rebuild();
        }

        [ContextMenu("Rebuild Now")]
        public void Rebuild()
        {
            items.Clear();

            if (!contentRoot || !itemPrefab)
            {
                Debug.LogError("[QuestTaskListUI] contentRoot / itemPrefab belum diassign.");
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            var planned = runner != null ? runner.GetPlannedSteps() : new List<(string id, string title)>();

            foreach (var (id, title) in planned)
            {
                var go = Instantiate(itemPrefab, contentRoot, false);
                var it = go.GetComponent<QuestTaskItem>();
                if (!it)
                {
                    Debug.LogError("[QuestTaskListUI] itemPrefab tidak memiliki QuestTaskItem.");
                    Destroy(go);
                    continue;
                }

                var label = string.IsNullOrEmpty(title) ? id : title;
                it.Setup(id, label);
                items[id] = it;
            }
        }

        // ===== UnityEvent hooks (bind dari QuestRunner di Inspector) =====

        public void HandleQuestBuilt()
        {
            Rebuild();
        }

        public void HandleStepStarted(string stepId)
        {
            // clear old active
            if (!string.IsNullOrEmpty(lastActiveId) && items.TryGetValue(lastActiveId, out var prev))
                prev.SetActive(false);

            // set new active
            if (items.TryGetValue(stepId, out var it))
                it.SetActive(true);

            lastActiveId = stepId;
        }

        public void HandleStepFinished(string stepId, bool isDone)
        {
            if (items.TryGetValue(stepId, out var it))
            {
                it.SetDone(isDone);
                it.SetActive(false);
            }
        }

        public void HandleQuestCompleted()
        {
            // Opsional: tampilkan banner/toast "Completed"
        }

        // ==== Util ====
        /// <summary>Update binding Runner dari script lain (opsional).</summary>
        public void SetRunner(QuestRunner r)
        {
            runner = r;
            Rebuild();
        }
    }
}
