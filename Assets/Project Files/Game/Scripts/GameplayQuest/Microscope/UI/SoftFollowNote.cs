using UnityEngine;
using UnityEngine.UI;

namespace FatahDev
{
    /// <summary>
    /// Menempatkan panel "catatan" di sisi FOV: jarak tetap, offset yaw tetap,
    /// smoothing, auto-fade di pusat, serta Pin/Unpin & Show/Hide.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SoftFollowNote : MonoBehaviour
    {
        [Header("References")]
        public Transform headTransform; // assign Main Camera (XR HMD)
        public CanvasGroup canvasGroup; // optional; untuk fade

        [Header("Placement")]
        [Tooltip("Jarak dari kepala (meter).")]
        public float followDistance = 0.95f;
        [Tooltip("Offset yaw relatif ke arah pandang HMD (derajat). Contoh: -25 = kiri, 25 = kanan.")]
        public float yawOffsetDegrees = -25f;
        [Tooltip("Offset tinggi terhadap HMD (meter). Negatif = sedikit di bawah mata.")]
        public float heightOffset = -0.05f;

        [Header("Behavior")]
        [Tooltip("Waktu smoothing gerak (detik). 0.1–0.2 nyaman.")]
        public float followSmoothing = 0.12f;
        [Tooltip("Panel selalu menghadap kamera di sumbu yaw (tanpa pitch/roll).")]
        public bool faceCameraYawOnly = true;
        [Tooltip("Minimal panel tetap berada di sisi (derajat dari pusat).")]
        public float minSideAngleFromCenter = 17f;
        [Tooltip("Sudut zona tengah tempat panel mulai redup (derajat).")]
        public float fadeCenterAngle = 10f;
        [Range(0f, 1f)] public float minAlphaInCenter = 0.35f;

        [Header("Visibility")]
        public bool startVisible = true;

        [Header("Debug")]
        public bool isPinned = false;

        private RectTransform rectTransform;
        private Vector3 velocity;

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (!headTransform && Camera.main) headTransform = Camera.main.transform;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (!headTransform && Camera.main) headTransform = Camera.main.transform;
            if (canvasGroup && !startVisible) canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            if (!headTransform) return;

            // 1) Arah forward rata (tanpa pitch) buat penempatan nyaman di VR
            Vector3 flatForward = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude < 0.0001f) flatForward = headTransform.forward;

            // 2) Terapkan offset yaw sehingga panel selalu di sisi FOV
            Quaternion yawOffset = Quaternion.AngleAxis(yawOffsetDegrees, Vector3.up);
            Vector3 sideDir = yawOffset * flatForward;

            // 3) Target posisi (jarak tetap + offset tinggi)
            Vector3 targetPos = headTransform.position + sideDir * followDistance;
            targetPos.y = headTransform.position.y + heightOffset;

            // 4) Jagain supaya panel nggak terlalu dekat pusat (minSideAngleFromCenter)
            float angleFromCenter = Vector3.SignedAngle(flatForward, (targetPos - headTransform.position).normalized, Vector3.up);
            float absAngle = Mathf.Abs(angleFromCenter);
            if (absAngle < minSideAngleFromCenter)
            {
                float sign = Mathf.Sign(yawOffsetDegrees == 0 ? angleFromCenter : yawOffsetDegrees);
                float corrected = minSideAngleFromCenter * sign;
                Quaternion clampYaw = Quaternion.AngleAxis(corrected, Vector3.up);
                Vector3 clampedDir = clampYaw * flatForward;
                targetPos = headTransform.position + clampedDir * followDistance;
                targetPos.y = headTransform.position.y + heightOffset;
                absAngle = minSideAngleFromCenter;
            }

            // 5) Smooth follow (kecuali jika dipin)
            if (!isPinned)
            {
                rectTransform.position = Vector3.SmoothDamp(rectTransform.position, targetPos, ref velocity, followSmoothing);
            }

            // 6) Hadapkan ke kamera (yaw-only agar stabil)
            if (faceCameraYawOnly)
            {
                Vector3 toCam = headTransform.position - rectTransform.position;
                Vector3 flatToCam = Vector3.ProjectOnPlane(toCam, Vector3.up);
                if (flatToCam.sqrMagnitude > 0.0001f)
                    rectTransform.rotation = Quaternion.LookRotation(flatToCam, Vector3.up);
            }
            else
            {
                rectTransform.LookAt(headTransform, Vector3.up);
                rectTransform.rotation = Quaternion.Euler(0f, rectTransform.eulerAngles.y, 0f);
            }

            // 7) Auto-fade bila terlalu ke pusat FOV
            if (canvasGroup)
            {
                float t = Mathf.InverseLerp(fadeCenterAngle, minSideAngleFromCenter, absAngle);
                float targetAlpha = Mathf.Lerp(minAlphaInCenter, 1f, t);
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * 10f);
            }
        }

        // === Public controls ===
        public void TogglePinned() => isPinned = !isPinned;
        public void SetPinned(bool pinned) => isPinned = pinned;

        public void ToggleVisible()
        {
            if (!canvasGroup) return;
            bool show = canvasGroup.alpha < 0.5f;
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
        }

        public void SnapLeft()  { yawOffsetDegrees = -Mathf.Abs(yawOffsetDegrees == 0 ? 25f : yawOffsetDegrees); }
        public void SnapRight() { yawOffsetDegrees =  Mathf.Abs(yawOffsetDegrees == 0 ? 25f : yawOffsetDegrees); }
    }
}
