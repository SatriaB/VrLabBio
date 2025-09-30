using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    // Knob mengatur Blur (fokus) -> _BlurStrength di shader "UI/MicroscopeRaw"
    // Zoom & efek lain TIDAK disentuh.
    public class RawImageLightDriver : MonoBehaviour
    {
        [SerializeField] private RawImage target;

        [Header("Blur (0 = tajam, 1 = blur)")]
        [SerializeField] private float minBlur = 0f;   // nilai ke _BlurStrength saat t=0
        [SerializeField] private float maxBlur = 1f;   // nilai ke _BlurStrength saat t=1

        [Header("Fokus V-shape (opsional)")]
        [SerializeField] private bool twoSidedFocus = false; // ON: tajam di tengah, kiri/kanan makin blur
        [Range(0,1)] [SerializeField] private float focusCenter = 0.5f;
        [Range(0.05f,1f)] [SerializeField] private float focusWidth = 0.5f;

        Material _mat;

        void Awake()
        {
            if (!target) target = GetComponent<RawImage>();
            _mat = Instantiate(target.material);
            target.material = _mat;
        }

        // Tetap panggil ini dari knob (0..1)
        public void SetNormalized(float t)
        {
            t = Mathf.Clamp01(t);

            // ---- BLUR yang baru ----
            float blur;
            if (twoSidedFocus)
            {
                float d = Mathf.Abs(t - focusCenter) / Mathf.Max(0.0001f, focusWidth);
                blur = Mathf.Lerp(minBlur, maxBlur, Mathf.Clamp01(d));
            }
            else
            {
                blur = Mathf.Lerp(minBlur, maxBlur, t);
            }
            
            Debug.Log("blur: " + blur);
            _mat.SetFloat("_BlurStrength", blur);
        }
    }
}
