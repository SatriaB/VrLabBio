using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FatahDev
{
    public class CaptureService : MonoBehaviour
    {
        public static CaptureService Instance { get; private set; }

        [Header("Default Options")]
        [SerializeField] private CaptureOptions defaultOptions = new CaptureOptions();

        [Header("Events")]
        public CaptureSavedUnityEvent onCaptureSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RequestCapture(Camera sourceCamera, string moduleName, 
                                   Dictionary<string, object> metadata,
                                   CaptureOptions overrideOptions = null)
        {
            if (sourceCamera == null)
            {
                Debug.LogWarning("[Capture] Source Camera is null. Assign a camera in GenericCaptureProvider.");
                return;
            }
            var opts = overrideOptions ?? defaultOptions;
            StartCoroutine(CaptureRoutine(sourceCamera, moduleName, metadata, opts));
        }

        private IEnumerator CaptureRoutine(Camera cam, string moduleName, 
                                           Dictionary<string, object> metadata, CaptureOptions opt)
        {
            yield return new WaitForEndOfFrame();

            // 1) Render kamera ke RT
            int width, height;
            if (opt.useSquareCrop)
            {
                width = height = Mathf.Max(64, opt.squareSize);
            }
            else
            {
                // fallback ke viewport kamera
                var vp = cam.rect; 
                width = Mathf.Max(64, Mathf.RoundToInt(Screen.width * vp.width));
                height = Mathf.Max(64, Mathf.RoundToInt(Screen.height * vp.height));
            }

            var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevActive = RenderTexture.active;
            var prevTarget = cam.targetTexture;

            Texture2D tex = null;
            try
            {
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;

                tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(rt);
            }

            // 2) Path & nama file
            string safeModule = string.IsNullOrEmpty(moduleName) ? "Unknown" : Sanitize(moduleName);
            string timestamp = System.DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
            string shortLabel = TryGetShortLabel(metadata); // opsional, mis. "40x" / "12.34mm" / "1.000g"

            string baseDir = Path.Combine(Application.persistentDataPath, opt.rootFolder, safeModule);
            Directory.CreateDirectory(baseDir);

            string baseName = string.IsNullOrEmpty(shortLabel) 
                ? $"{safeModule}_{timestamp}" 
                : $"{safeModule}_{timestamp}_{Sanitize(shortLabel)}";

            string pngPath = Path.Combine(baseDir, baseName + ".png");
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Destroy(tex);

            // 3) Metadata JSON
            string metaJson = BuildMetadataJson(moduleName, pngPath, metadata);
            File.WriteAllText(Path.Combine(baseDir, baseName + ".json"), metaJson, Encoding.UTF8);

            Debug.Log($"[Capture] Saved: {pngPath}");
            onCaptureSaved?.Invoke(moduleName, pngPath, metaJson);
        }

        private static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_');
        }

        private static string TryGetShortLabel(Dictionary<string, object> meta)
        {
            if (meta == null) return null;
            if (meta.TryGetValue("label", out var l) && l != null) return l.ToString();
            if (meta.TryGetValue("magnification", out var m) && m != null) return m.ToString() + "x";
            if (meta.TryGetValue("reading_mm", out var r) && r != null) return r.ToString() + "mm";
            if (meta.TryGetValue("weight_g", out var w) && w != null) return w.ToString() + "g";
            return null;
        }

        private static string BuildMetadataJson(string module, string imagePath, Dictionary<string, object> meta)
        {
            var dict = new Dictionary<string, object>
            {
                { "module", module },
                { "image_path", imagePath },
                { "timestamp_local", System.DateTimeOffset.Now.ToString("o") },
                { "timestamp_unix_ms", System.DateTimeOffset.Now.ToUnixTimeMilliseconds() }
            };

            if (meta != null)
                foreach (var kv in meta) dict[kv.Key] = kv.Value;

            return MiniJson.Serialize(dict);
        }
    }
}
