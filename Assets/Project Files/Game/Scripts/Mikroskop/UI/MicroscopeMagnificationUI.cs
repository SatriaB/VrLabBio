using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    /// <summary>
    /// Ganti icon pembesaran saat MikroskopLensBehaviour ganti step.
    /// Diisi sprite per-index sesuai urutan lensProfiles di MikroskopLensBehaviour.
    /// </summary>
    public class MicroscopeMagnificationUI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private bool setNativeSize = true;

        [Header("Sprites by index (match lensProfiles order)")]
        [Tooltip("Isi urutan: [0]=4x, [1]=10x, [2]=40x, [3]=100x (atau sesuai urutan lensProfiles kamu).")]
        [SerializeField] private Sprite[] spritesByIndex = new Sprite[4];

        private void Reset()
        {
            if (!targetImage) targetImage = GetComponentInChildren<Image>();
        }

        /// <summary>Dipanggil dari MikroskopLensBehaviour ketika stepIndex berubah.</summary>
        public void SetStepIndex(int stepIndex)
        {
            if (!targetImage || spritesByIndex == null || spritesByIndex.Length == 0) return;

            int idx = Mathf.Clamp(stepIndex, 0, spritesByIndex.Length - 1);
            var sprite = spritesByIndex[idx];
            if (sprite == null || targetText == null) return;

            int[] mags = { 4, 10, 40, 100 };
            int objective = mags[Mathf.Clamp(idx, 0, mags.Length - 1)];
            targetText.text = $"PERBESARAN {objective}X";
            
            targetImage.sprite = sprite;
            if (setNativeSize) targetImage.SetNativeSize();
        }
    }
}