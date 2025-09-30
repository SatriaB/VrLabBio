using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private Slider sliderMasterVolume;
        [SerializeField] private Toggle toggleHaptic;
        [SerializeField] private TMP_Text volumeValueText;

        private void OnEnable()
        {
            // default sesi
            if (sliderMasterVolume)
            {
                sliderMasterVolume.SetValueWithoutNotify(AudioListener.volume);
                UpdateVolumeLabel(AudioListener.volume);
            }
            if (toggleHaptic)
            {
                toggleHaptic.SetIsOnWithoutNotify(true); // default ON per sesi
            }
        }

        public void OnMasterVolumeChanged(float v)
        {
            AudioListener.volume = Mathf.Clamp01(v);
            UpdateVolumeLabel(AudioListener.volume);
            TryHapticLight();
        }

        public void OnHapticToggled(bool on)
        {
            // di mode kios kita cuma flag lokal; panggil haptic ringan sebagai feedback
            if (on) TryHapticLight();
        }

        private void UpdateVolumeLabel(float v)
        {
            if (volumeValueText) volumeValueText.text = Mathf.RoundToInt(v * 100f) + "%";
        }

        private void TryHapticLight()
        {
            // panggil sistem haptic milikmu kalau ada (biar gak error kalau tidak ada)
            // contoh:
            // Haptic.Play(Haptic.HAPTIC_LIGHT);
        }
    }
}