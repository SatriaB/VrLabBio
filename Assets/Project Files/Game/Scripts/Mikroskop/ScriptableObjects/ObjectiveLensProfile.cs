using UnityEngine;

namespace FatahDev
{
    [CreateAssetMenu(menuName="Microscope/Objective Lens Profile")]
    public class ObjectiveLensProfile : ScriptableObject
    {
        public string displayName = "4x";
        [Range(1,200)] public int magnification = 4;     // 4,10,40,100
        [Range(0f,1f)] public float brightnessMul = 1f;  // lamp/intensity multiplier
        [Range(0f,5f)] public float focusDepth = 1f;     // simulate DOF (pakai shader)
        public bool oilImmersion = false;                // true untuk 100x oil

        public string magnificationProp = "_Magnification";
        public string focusDepthProp   = "_FocusDepth";
        public string vignetteProp     = "_Vignette";
        public string aberrationProp   = "_ChromAb";
    }
}
