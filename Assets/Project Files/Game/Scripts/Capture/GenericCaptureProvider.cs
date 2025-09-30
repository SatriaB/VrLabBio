using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    /// <summary>
    /// Pasang di root tiap alat. Isi Module Name & Camera sumber.
    /// Update metadata runtime via SetText/SetNumber/SetLabel.
    /// </summary>
    public class GenericCaptureProvider : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string moduleName = "Microscope"; // ganti: Caliper / Micrometer / Balance / ...

        [Header("Source Camera (wajib)")]
        [Tooltip("Kamera yang ingin di-capture (mis. kamera mikroskop / main XR camera).")]
        [SerializeField] private Camera sourceCamera;

        [Header("Options")]
        [SerializeField] private bool useSquareCrop = true;
        [SerializeField] private int squareSize = 1024;

        [Header("Auto Active")]
        [SerializeField] private bool setActiveOnEnable = true;

        private readonly Dictionary<string, object> metadata = new Dictionary<string, object>();

        public string ModuleName => moduleName;
        public Camera SourceCamera => sourceCamera;

        private void OnEnable()
        {
            if (setActiveOnEnable) CaptureRouter.SetActiveProvider(this);
        }

        private void OnDisable()
        {
            if (CaptureRouter.ActiveProvider == this)
                CaptureRouter.SetActiveProvider(null);
        }

        public void SetText(string key, string value)
        {
            if (!string.IsNullOrEmpty(key)) metadata[key] = value ?? "";
        }

        public void SetNumber(string key, float value)
        {
            if (!string.IsNullOrEmpty(key)) metadata[key] = Mathf.Round(value * 1000f) / 1000f;
        }

        public void SetLabel(string shortLabel)
        {
            if (!string.IsNullOrEmpty(shortLabel)) metadata["label"] = shortLabel;
        }

        public Dictionary<string, object> SnapshotMetadata() => new Dictionary<string, object>(metadata);

        public CaptureOptions BuildOptions() => new CaptureOptions
        {
            useSquareCrop = useSquareCrop,
            squareSize = squareSize,
            rootFolder = "Captures"
        };
    }
}
