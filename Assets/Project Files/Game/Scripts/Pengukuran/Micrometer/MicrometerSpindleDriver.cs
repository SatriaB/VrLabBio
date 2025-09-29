// FatahDev/MicrometerSpindleDriver.cs
// Map value 0..1 (dari knob/thimble) -> posisi local spindle (min..max).
using UnityEngine;

namespace FatahDev
{
    public class MicrometerSpindleDriver : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform spindle;       // drag obj Spindle di sini

        [Header("Travel (local space)")]
        [SerializeField] private Vector3 localMin;        // posisi terbuka
        [SerializeField] private Vector3 localMax;        // posisi jepit

        [Header("Feel")]
        [SerializeField] private bool smooth = true;
        [SerializeField, Min(0f)] private float smoothSpeed = 12f;

        // cache
        private float target01;

        /// <summary>Terima nilai 0..1 dari knob/thimble.</summary>
        public void SetValue(float value01)
        {
            target01 = Mathf.Clamp01(value01);
            if (!smooth) ApplyImmediate();
        }

        private void Update()
        {
            if (!spindle) return;
            if (!smooth) return;

            // lerp halus menuju posisi target
            Vector3 to = Vector3.Lerp(localMin, localMax, target01);
            spindle.localPosition = Vector3.Lerp(spindle.localPosition, to, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        }

        private void ApplyImmediate()
        {
            if (!spindle) return;
            spindle.localPosition = Vector3.Lerp(localMin, localMax, target01);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spindle && Application.isEditor && !Application.isPlaying)
            {
                // preview di editor saat atur inspector
                spindle.localPosition = Vector3.Lerp(localMin, localMax, target01);
            }
        }
#endif
    }
}