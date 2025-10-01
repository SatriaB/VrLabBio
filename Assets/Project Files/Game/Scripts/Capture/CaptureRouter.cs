using UnityEngine;

namespace FatahDev
{
    /// <summary> Menentukan provider aktif (alat yang sedang dipakai). </summary>
    public static class CaptureRouter
    {
        public static GenericCaptureProvider ActiveProvider { get; private set; }

        public static void SetActiveProvider(GenericCaptureProvider provider)
        {
            ActiveProvider = provider;
            if (provider != null)
                Debug.Log($"[Capture] Active: {provider.ModuleName} ({provider.name})");
        }
    }
}