using UnityEngine;

namespace FatahDev
{
    public class MeasurementTarget : MonoBehaviour
    {
        [Header("Reference Data")]
        public string targetId = "Cube_25mm";
        public float groundTruthMm = 25.00f;
        public float toleranceMm = 0.10f;

        [Header("(Opsional) Offset Snap Pose")]
        public Transform measurementAttachOffset;
    }
}