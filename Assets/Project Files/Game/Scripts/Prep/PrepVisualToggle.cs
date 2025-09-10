using UnityEngine;

namespace FatahDev
{
    public class PrepVisualToggle : MonoBehaviour
    {
        [Tooltip("Irisan di kaca benda (awal: nonaktif).")]
        public GameObject sliceOnSlide;

        [Tooltip("Cap tipis di atas singkong (awal: aktif).")]
        public GameObject singkongCap;

        [Tooltip("Garis potong/decal (awal: nonaktif).")]
        public GameObject cutMark;

        public void ApplySliceVisuals()
        {
            if (sliceOnSlide) sliceOnSlide.SetActive(true);
            if (singkongCap) singkongCap.SetActive(false);
            if (cutMark) cutMark.SetActive(true);
        }
    }
}