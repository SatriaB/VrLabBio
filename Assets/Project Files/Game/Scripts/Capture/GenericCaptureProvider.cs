using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    /// <summary>
    /// Pasang di root tiap alat. Isi Module Name & Camera sumber.
    /// Update metadata runtime via SetText/SetNumber/SetLabel.
    /// </summary>
    public class GenericCaptureProvider : MonoBehaviour
    {
        [Header("Identity")] [SerializeField]
        private string moduleName = "Microscope"; // ganti: Caliper / Micrometer / Balance / ...

        [Header("Source Camera (wajib)")]
        [Tooltip("Kamera yang ingin di-capture (mis. kamera mikroskop / main XR camera).")]
        [SerializeField]
        private Camera sourceCamera;

        [Header("Options")] [SerializeField] private bool useSquareCrop = true;
        [SerializeField] private int squareSize = 1024;

        [Header("Auto Active")] [SerializeField]
        private bool setActiveOnEnable = true;

        private readonly Dictionary<string, object> metadata = new Dictionary<string, object>();

        public string ModuleName => moduleName;
        public Camera SourceCamera => sourceCamera;

        [SerializeField] private UnityEvent onCapture;

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


        #region AUTO UPLOAD → VRLWorks

        [Header("Auto Upload")] [SerializeField]
        private bool autoUpload = true;

        [SerializeField] private WorkStepGroupId defaultGroup = WorkStepGroupId.Microscope;
        [SerializeField] private int fallbackWorkStepId = 0;
        [SerializeField] private MonoBehaviour workStepIdSource; // opsional: sumber stepId dinamis

        [Header("File")] [SerializeField] private bool useJpeg = true;
        [Range(50, 100)] [SerializeField] private int jpegQuality = 90;
        [SerializeField] private string fileNamePrefix = "capture_"; // akan ditambah timestamp

// === PANGGIL salah satu dari 3 handler berikut, sesuai event milikmu ===

// 1) Kalau eventmu kirim Texture2D SAJA
        public void AutoUpload_OnCapturedTexture(Texture2D tex)
        {
            if (!autoUpload || !tex) return;
            ResolveWorkTarget(out var stepId, out int? fieldId);
            UploadTex(tex, defaultGroup, stepId);
        }

// 2) Kalau eventmu kirim Texture2D + group (int) -> 0:Microscope,1:Caliper,2:Micrometer,3:AnalyticalBalance
        private void AutoUpload_OnCapturedTextureWithGroup(Texture2D tex, int captureGroup)
        {
            if (!autoUpload || !tex) return;
            var group = MapGroupIntToEnum(captureGroup);
            int stepId = ResolveWorkStepId();
            UploadTex(tex, group, stepId);
        }

// 3) Kalau eventmu kirim byte[] + group (provider sudah encode sendiri)
        private void AutoUpload_OnCapturedBytesWithGroup(byte[] bytes, int captureGroup)
        {
            if (!autoUpload || bytes == null || bytes.Length == 0) return;
            var group = MapGroupIntToEnum(captureGroup);
            int stepId = ResolveWorkStepId();

            string ts = System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string name = fileNamePrefix + ts + (useJpeg ? ".jpg" : ".png");
            VRLWorks.UploadFile(group, stepId, bytes, name, useJpeg ? "image/jpeg" : "image/png",
                (ok, res) => UnityEngine.Debug.Log(ok ? $"[Upload OK] {res}" : $"[Upload ERR] {res}"));
        }

// ===== inti upload dari Texture2D =====
        private void UploadTex(Texture2D tex, WorkStepGroupId group, int workStepId)
        {
            // cari stepId + fieldId dari context aktif (workStepIdSource / objek step)
            ResolveWorkTarget(out var resolvedStepId, out int? fieldId);
            if (resolvedStepId > 0) workStepId = resolvedStepId;

            string ts = System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            if (useJpeg)
            {
                string name = $"{fileNamePrefix}{ts}.jpg";
                VRLWorks.UploadTextureJPG(group, workStepId, tex, jpegQuality, name, fieldId,
                    (ok, res) => UnityEngine.Debug.Log(ok ? $"[Upload OK] {res}" : $"[Upload ERR] {res}"));
            }
            else
            {
                string name = $"{fileNamePrefix}{ts}.png";
                VRLWorks.UploadTexturePNG(group, workStepId, tex, name, fieldId,
                    (ok, res) => UnityEngine.Debug.Log(ok ? $"[Upload OK] {res}" : $"[Upload ERR] {res}"));
            }

            onCapture.Invoke();
        }

// ===== helper: ambil workStepId aktif (dinamis kalau ada) =====
        private int ResolveWorkStepId()
        {
            if (!workStepIdSource) return fallbackWorkStepId;
            var t = workStepIdSource.GetType();

            var mi = t.GetMethod("GetCurrentWorkStepId",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (mi != null && mi.ReturnType == typeof(int) && mi.GetParameters().Length == 0)
            {
                try
                {
                    return (int)mi.Invoke(workStepIdSource, null);
                }
                catch
                {
                }
            }

            var pi = t.GetProperty("CurrentWorkStepId",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (pi != null && pi.PropertyType == typeof(int))
            {
                try
                {
                    return (int)pi.GetValue(workStepIdSource);
                }
                catch
                {
                }
            }

            var fi = t.GetField("CurrentWorkStepId",
                         System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.NonPublic)
                     ?? t.GetField("workStepId",
                         System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.NonPublic);
            if (fi != null && fi.FieldType == typeof(int))
            {
                try
                {
                    return (int)fi.GetValue(workStepIdSource);
                }
                catch
                {
                }
            }

            return fallbackWorkStepId;
        }

// ===== helper: mapping group int -> enum =====
        private static WorkStepGroupId MapGroupIntToEnum(int g)
        {
            switch (g)
            {
                case 3: return WorkStepGroupId.Microscope;
                case 5: return WorkStepGroupId.Caliper;
                case 6: return WorkStepGroupId.Micrometer;
                case 4: return WorkStepGroupId.AnalyticalBalance;
                default: return WorkStepGroupId.Microscope;
            }
        }

        static object GetPF(object obj, string name)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Instance |
                                                     System.Reflection.BindingFlags.Public |
                                                     System.Reflection.BindingFlags.NonPublic;
            var pi = t.GetProperty(name, F);
            if (pi != null) return pi.GetValue(obj);
            var fi = t.GetField(name, F);
            if (fi != null) return fi.GetValue(obj);
            return null;
        }

        private void ResolveWorkTarget(out int stepId, out int? fieldId)
        {
            stepId = fallbackWorkStepId;
            fieldId = null;

            if (!workStepIdSource)
                return;

            // --- 2A. Coba method/property langsung untuk STEP ID ---
            // Method GetCurrentWorkStepId()
            var t = workStepIdSource.GetType();
            var F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic;

            var mi = t.GetMethod("GetCurrentWorkStepId", F);
            if (mi != null && mi.ReturnType == typeof(int) && mi.GetParameters().Length == 0)
            {
                try
                {
                    stepId = (int)mi.Invoke(workStepIdSource, null);
                }
                catch
                {
                }
            }
            else
            {
                // Property/Field CurrentWorkStepId / workStepId
                var pi = t.GetProperty("CurrentWorkStepId", F);
                if (pi != null && pi.PropertyType == typeof(int))
                {
                    try
                    {
                        stepId = (int)pi.GetValue(workStepIdSource);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    var fi = t.GetField("CurrentWorkStepId", F) ?? t.GetField("workStepId", F);
                    if (fi != null && fi.FieldType == typeof(int))
                    {
                        try
                        {
                            stepId = (int)fi.GetValue(workStepIdSource);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            // --- 2B. Coba method/property langsung untuk FIELD ID (file field) ---
            // Method GetCurrentFileFieldId()/GetCurrentFieldId()
            var mField = t.GetMethod("GetCurrentFileFieldId", F) ?? t.GetMethod("GetCurrentFieldId", F);
            if (mField != null && mField.ReturnType == typeof(int) && mField.GetParameters().Length == 0)
            {
                try
                {
                    fieldId = (int)mField.Invoke(workStepIdSource, null);
                    return;
                }
                catch
                {
                }
            }

            // Property CurrentFileFieldId/CurrentFieldId
            var pField = t.GetProperty("CurrentFileFieldId", F) ?? t.GetProperty("CurrentFieldId", F);
            if (pField != null && pField.PropertyType == typeof(int))
            {
                try
                {
                    fieldId = (int)pField.GetValue(workStepIdSource);
                    return;
                }
                catch
                {
                }
            }

            var fField = t.GetField("CurrentFileFieldId", F) ?? t.GetField("CurrentFieldId", F);
            if (fField != null && fField.FieldType == typeof(int))
            {
                try
                {
                    fieldId = (int)fField.GetValue(workStepIdSource);
                    return;
                }
                catch
                {
                }
            }

            // --- 2C. Coba ambil dari objek step aktif (mirip JSON kamu: step.field.id / step.fields[i].result.file) ---
            // Cari container step: CurrentWorkStep / ActiveWorkStep / WorkStep / step
            object stepObj = GetPF(workStepIdSource, "CurrentWorkStep")
                             ?? GetPF(workStepIdSource, "ActiveWorkStep")
                             ?? GetPF(workStepIdSource, "WorkStep")
                             ?? GetPF(workStepIdSource, "step");

            if (stepObj != null)
            {
                // override stepId jika ada "id"
                var stepIdObj = GetPF(stepObj, "id");
                if (stepIdObj is int sId) stepId = sId;

                // 1) Struktur tunggal: step.field.id
                var fieldObj = GetPF(stepObj, "field");
                if (fieldObj != null)
                {
                    var fid = GetPF(fieldObj, "id");
                    if (fid is int f)
                    {
                        fieldId = f;
                        return;
                    }
                }

                // 2) Struktur list: step.fields[].result.file
                var fieldsObj = GetPF(stepObj, "fields");
                if (fieldsObj is System.Collections.IEnumerable enumerable)
                {
                    foreach (var f in enumerable)
                    {
                        var rid = GetPF(f, "id");
                        var result = GetPF(f, "result"); // result: { file, text, score }
                        if (result != null)
                        {
                            var fileProp = GetPF(result, "file"); // bisa string/null
                            // kalau ada properti 'file', anggap ini field file
                            var hasFileMember = result.GetType().GetProperty("file", F) != null
                                                || result.GetType().GetField("file", F) != null;
                            if (hasFileMember && rid is int ridInt)
                            {
                                fieldId = ridInt;
                                return;
                            }
                        }

                        // alternatif: tipe/jenis field
                        var typeVal = GetPF(f, "type") ?? GetPF(f, "fieldType");
                        if (typeVal is string s && s.IndexOf("file", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (rid is int ridByType)
                            {
                                fieldId = ridByType;
                                return;
                            }
                        }
                    }
                }
            }

            // Jika tidak ketemu fieldId, biarkan null (server akan pilih default file-field step ini)
        }

        #endregion


        public Dictionary<string, object> SnapshotMetadata() => new Dictionary<string, object>(metadata);

        public CaptureOptions BuildOptions() => new CaptureOptions
        {
            useSquareCrop = useSquareCrop,
            squareSize = squareSize,
            rootFolder = "Captures"
        };

        public void switchId(int i)
        {
            fallbackWorkStepId = i;
        }
    }
}