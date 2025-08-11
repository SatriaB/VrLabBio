using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    public class RawImageLightDriver : MonoBehaviour
    {
        [SerializeField] private RawImage target; // RawImage yg menampilkan RT
        [Header("Ranges")]
        [SerializeField] private float minBrightness = 0.6f;
        [SerializeField] private float maxBrightness = 1.8f;
        [SerializeField] private float minGamma = 1.2f;   //  >1 = sedikit gelap (kontras naik)
        [SerializeField] private float maxGamma = 0.8f;   //  <1 = terasa lebih terang
        [SerializeField] private float minContrast = 0.95f;
        [SerializeField] private float maxContrast = 1.15f;

        [Header("Vignette (opsional)")]
        [SerializeField] private float minVig = 0.15f;
        [SerializeField] private float maxVig = 0.45f;

        Material _mat;

        void Awake()
        {
            if (!target) target = GetComponent<RawImage>();
            // Biar tidak mengubah material asset, kita instansiasi
            _mat = Instantiate(target.material);
            target.material = _mat;
        }

        // Hubungkan ke VRKnobInteractable.OnValueChanged(t)
        public void SetNormalized(float t)
        {
            t = Mathf.Clamp01(t);

            _mat.SetFloat("_Brightness", Mathf.Lerp(minBrightness, maxBrightness, t));
            _mat.SetFloat("_Gamma",      Mathf.Lerp(minGamma,      maxGamma,      t));
            _mat.SetFloat("_Contrast",   Mathf.Lerp(minContrast,   maxContrast,   t));
            _mat.SetFloat("_Vignette",   Mathf.Lerp(minVig,        maxVig,        t));
        }
    }
}