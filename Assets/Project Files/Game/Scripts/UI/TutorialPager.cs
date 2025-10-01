using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    public class TutorialPager : MonoBehaviour
    {
        [SerializeField] private Image imagePage;
        [SerializeField] private TMP_Text pageLabel;
        [SerializeField] private Sprite[] pages;

        private int index;

        private void OnEnable()
        {
            index = 0;
            Refresh();
        }

        public void OnPrev()
        {
            if (pages == null || pages.Length == 0) return;
            index = (index - 1 + pages.Length) % pages.Length;
            Refresh();
        }

        public void OnNext()
        {
            if (pages == null || pages.Length == 0) return;
            index = (index + 1) % pages.Length;
            Refresh();
        }

        private void Refresh()
        {
            if (imagePage) imagePage.sprite = (pages != null && pages.Length > 0) ? pages[Mathf.Clamp(index, 0, pages.Length - 1)] : null;
            if (pageLabel) pageLabel.text = (pages == null || pages.Length == 0) ? "0/0" : $"{index + 1}/{pages.Length}";
        }
    }
}