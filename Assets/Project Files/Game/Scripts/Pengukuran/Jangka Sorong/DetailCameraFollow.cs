using UnityEngine;

namespace FatahDev
{
    public class DetailCameraFollow : MonoBehaviour
    {
        [Header("Anchors (drag di Inspector)")]
        [SerializeField] private Transform sliderAnchor; 

        [Header("Offsets (lokal)")]
        [SerializeField] private Vector3 positionOffsetLocal = new Vector3(0f, 0.06f, -0.08f);


        private void LateUpdate()
        {
            if (sliderAnchor == null) return;

            transform.position = sliderAnchor.TransformPoint(positionOffsetLocal);
        }
    }
}