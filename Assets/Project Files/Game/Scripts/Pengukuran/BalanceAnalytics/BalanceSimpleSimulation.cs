using UnityEngine;
using TMPro;

namespace FatahDev
{
    public class BalanceDisplaySimple : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI displayText;

        [Header("Door / Ratio")]
        [SerializeField, Range(0f, 1f)] private float doorOpenRatio = 0f;
        [SerializeField, Range(0f, 1f)] private float closedThreshold = 0.15f;   // ambang tutup
        [SerializeField, Range(0f, 1f)] private float closedHysteresis = 0.05f;  // dead-zone

        [Header("Display Settings (gram)")]
        [SerializeField] private float readabilityG = 0.001f;   // 3 desimal
        [SerializeField] private float noiseOpenMaxG = 0.0040f; // noise hanya saat pintu terbuka

        [Header("Wind Bump (saat pintu dibuka)")]
        [SerializeField] private Vector2 bumpAmplitudeRangeG = new Vector2(0.003f, 0.006f);
        [SerializeField] private float bumpDecayTauSeconds = 0.6f;

        [Header("Stable Detection")]
        [SerializeField] private float stableWindowSeconds = 0.9f;
        [SerializeField] private float stableDeltaEpsilonG = 0.0005f;
        [SerializeField] private bool holdValueWhileStable = true;
        [SerializeField] private int  minSugarCount = 4; // opsional, target minimal

        [Header("Debug")]
        [SerializeField] private bool disableNoiseAndBump = false;

        // runtime (di-update HANYA via event)
        private int   sugarCount = 0;
        private float totalMassGram = 0f;

        private bool  isDoorClosed = true;
        private float bumpAmplitudeCurrentG = 0f;
        private float bumpTime = 0f;

        private float lastForStability = 0f;
        private float stableTimer = 0f;
        private bool  isStable = false;
        private float lastStableValueG = 0f;
        private bool  objectiveCompleted = false;

        // ===== DIPANGGIL DARI EVENT ROUTER PAN =====
        public void OnCountAndMassChanged(int count, float massG)
        {
            sugarCount    = count;
            totalMassGram = massG;
        }

        // ===== DIPANGGIL DARI EVENT DOOR =====
        public void SetDoorRatio(float ratio)
        {
            doorOpenRatio = Mathf.Clamp01(ratio);

            bool wasClosed = isDoorClosed;

            if (isDoorClosed)
            {
                // CLOSED -> OPEN butuh lewat ambang + histeresis
                if (doorOpenRatio > closedThreshold + closedHysteresis)
                    isDoorClosed = false;
            }
            else
            {
                // OPEN -> CLOSED kalau turun <= ambang
                if (doorOpenRatio <= closedThreshold)
                    isDoorClosed = true;
            }

            if (isDoorClosed != wasClosed)
            {
                stableTimer = 0f; // reset deteksi
                if (isDoorClosed)
                {
                    // Tutup -> matikan bump
                    bumpAmplitudeCurrentG = 0f;
                    bumpTime = 0f;
                }
                else
                {
                    // Buka -> mulai "angin"
                    bumpAmplitudeCurrentG = disableNoiseAndBump
                        ? 0f
                        : Random.Range(bumpAmplitudeRangeG.x, bumpAmplitudeRangeG.y);
                    bumpTime = 0f;
                    isStable = false;
                }
            }
        }

        private void Update()
        {
            // Noise & bump hanya ketika pintu terbuka
            float noise = 0f, bump = 0f;

            if (!isDoorClosed && !disableNoiseAndBump)
            {
                noise = Random.Range(-noiseOpenMaxG, noiseOpenMaxG);

                if (bumpAmplitudeCurrentG > 0f)
                {
                    bumpTime += Time.deltaTime;
                    bump = bumpAmplitudeCurrentG *
                           Mathf.Exp(-bumpTime / Mathf.Max(0.0001f, bumpDecayTauSeconds));
                }
            }

            // Hitung & bulatkan
            float raw       = totalMassGram + noise + bump;
            float displayed = Mathf.Round(raw / readabilityG) * readabilityG;

            // Deteksi STABLE (hanya saat pintu tertutup)
            if (isDoorClosed && Mathf.Abs(displayed - lastForStability) < stableDeltaEpsilonG)
                stableTimer += Time.deltaTime;
            else
                stableTimer = 0f;

            lastForStability = displayed;

            bool shouldBeStable = isDoorClosed && stableTimer >= stableWindowSeconds;
            if (shouldBeStable != isStable)
            {
                isStable = shouldBeStable;
                if (isStable) lastStableValueG = displayed;
            }

            // Bekukan angka saat STABLE + pintu tertutup
            if (holdValueWhileStable && isStable && isDoorClosed)
                displayed = lastStableValueG;

            if (displayText) displayText.text = displayed.ToString("F3") + " g";

            // (Opsional) objective selesai
            if (!objectiveCompleted && isStable && sugarCount >= minSugarCount)
                objectiveCompleted = true;
        }
    }
}
