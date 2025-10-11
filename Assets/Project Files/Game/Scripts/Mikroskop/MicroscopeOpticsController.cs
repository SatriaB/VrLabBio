using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    public class MicroscopeOpticsController : MonoBehaviour
    {
        [Header("UI Target")]
        public RawImage targetImage;


        public void Apply(ObjectiveLensProfile p, GenericCaptureProvider capture = null)
        {
            if (targetImage.material == null || p == null) return;
            
            Debug.Log(targetImage.material);

            targetImage.material.SetFloat("_Zoom", Mathf.Lerp(1.0f, 3.5f, Mathf.InverseLerp(4, 100, p.magnification)));
            targetImage.material.SetFloat("_Vignette", Mathf.Lerp(0.15f, 0.35f, Mathf.InverseLerp(4, 100, p.magnification)));
            targetImage.material.SetFloat("_ChromAb", Mathf.Lerp(0.005f, 0.02f, Mathf.InverseLerp(4, 100, p.magnification)));
            targetImage.material.SetFloat("_BlurStrength", Mathf.InverseLerp(4, 100, p.magnification) * p.focusDepth * 0.5f);
            targetImage.material.SetFloat("_Brightness", p.brightnessMul);

            switch (p.displayName)
            {
                case "4x":
                    capture.switchId(35);
                    break;
                case "10x":
                    capture.switchId(40);
                    break;
                case "40x":
                    capture.switchId(45);
                    break;
                case "100x":
                    capture.switchId(51);
                    break;
                default:
                    break;
            }

            if (p.oilImmersion)
            {
                bool oilPlaced = true; // TODO: connect ke gameplay
                if (!oilPlaced)
                    targetImage.material.SetFloat("_Brightness", p.brightnessMul * 0.6f);
            }
        }
    }

}