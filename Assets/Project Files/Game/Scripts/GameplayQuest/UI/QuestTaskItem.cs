using UnityEngine;
using TMPro;

namespace FatahDev
{
    public class QuestTaskItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        [SerializeField] private GameObject doneIcon;   // ✓
        [SerializeField] private GameObject pendingIcon; // • (atau kosong)
        [SerializeField] private GameObject activeMark; // optional: highlight step aktif

        [SerializeField] private string stepId;
        public string StepId => stepId;

        public void Setup(string id, string label)
        {
            stepId = id;
            if (labelText) labelText.text = label;
            SetDone(false);
            SetActive(false);
        }

        public void SetDone(bool done)
        {
            if (doneIcon)   doneIcon.SetActive(done);
            if (pendingIcon) pendingIcon.SetActive(!done);
        }

        public void SetActive(bool active)
        {
            if (activeMark) activeMark.SetActive(active);
        }
    }
}