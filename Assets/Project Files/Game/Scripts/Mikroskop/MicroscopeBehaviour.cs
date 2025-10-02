using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace FatahDev
{
    public class MicroscopeDropGuard : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Rigidbody utama mikroskop.")]
        public Rigidbody microscopeRigidbody;

        [Tooltip("Titik dasar mikroskop untuk ground-check (biasanya base).")]
        public Transform baseTransform;

        [Tooltip("Collider trigger zona dock (lebih toleran saat berada di sini).")]
        public Collider dockZone;

        [Header("Layers & Ground Check")]
        public LayerMask groundLayers = ~0;
        [Tooltip("Radius sphere untuk ground check.")]
        public float groundCheckRadius = 0.06f;
        [Tooltip("Jarak sphere-cast ke bawah dari base.")]
        public float groundCheckDistance = 0.10f;

        [Header("Impact Fail")]
        [Tooltip("Kecepatan relatif tabrakan yg dianggap FAIL (m/s).")]
        public float impactFailSpeed = 3.0f;

        [Header("Fall Fail")]
        [Tooltip("Drop height minimum untuk FAIL saat mendarat (meter).")]
        public float minFallHeightForFail = 0.35f;
        [Tooltip("Drop height untuk WARNING (meter).")]
        public float minMinorDropToWarn = 0.18f;

        [Header("Tilt Fail (di luar Dock)")]
        [Tooltip("Derajat miring untuk mulai WARNING.")]
        public float tiltWarningDegrees = 35f;
        [Tooltip("Derajat miring untuk FAIL jika ditahan.")]
        public float tiltFailDegrees = 45f;
        [Tooltip("Durasi tahan > tiltWarningDegrees untuk WARNING (detik).")]
        public float tiltWarningHold = 1.5f;
        [Tooltip("Durasi tahan > tiltFailDegrees untuk FAIL (detik).")]
        public float tiltFailHold = 1.0f;

        [Header("Control")]
        [Tooltip("Kalau false, guard diabaikan (bisa diaktifkan saat step tertentu).")]
        public bool guardEnabled = true;

        [Header("Events")]
        public UnityEvent<string> OnWarning;
        public UnityEvent<string> OnFail;

        // ---- Runtime state ----
        bool isDocked;
        bool wasGrounded;
        bool falling;
        float lastGroundedY;
        float fallStartY;
        float tiltWarningTimer;
        float tiltFailTimer;
        float warnCooldown;
        const float WarnCooldownSeconds = 0.8f;

        void Reset()
        {
            microscopeRigidbody = GetComponentInParent<Rigidbody>();
            if (!baseTransform) baseTransform = transform;
        }

        void Awake()
        {
            if (!microscopeRigidbody)
                Debug.LogWarning("[DropGuard] Missing Rigidbody reference.");
            if (!baseTransform) baseTransform = transform;
        }

        void Update()
        {
            if (!guardEnabled) return;

            GroundCheckAndDropLogic();
            TiltLogic();

            if (warnCooldown > 0f) warnCooldown -= Time.deltaTime;
        }

        // -------- Dock zone detection --------
        void OnTriggerEnter(Collider other)
        {
            if (!guardEnabled) return;
            if (dockZone && other == dockZone) isDocked = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (!guardEnabled) return;
            if (dockZone && other == dockZone) isDocked = false;
        }

        // -------- Impact detection --------
        void OnCollisionEnter(Collision collision)
        {
            if (!guardEnabled) return;
            if (microscopeRigidbody == null) return;

            // Abaikan tabrakan dengan bagian sendiri
            if (collision.transform.root == transform.root) return;

            // Filter layer permukaan (meja/lantai)
            bool hitValidLayer = ((groundLayers.value & (1 << collision.gameObject.layer)) != 0);
            if (!hitValidLayer) return;

            float relSpeed = collision.relativeVelocity.magnitude;
            if (relSpeed >= impactFailSpeed)
            {
                Fail("Microscope.Fail.Drop",
                    $"Percobaan gagal: Mikroskop terbanting (impact {relSpeed:0.00} m/s).");
            }
        }

        // -------- Ground & fall logic --------
        void GroundCheckAndDropLogic()
        {
            Vector3 origin = baseTransform.position + Vector3.up * (groundCheckRadius + 0.01f);
            bool isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down,
                                                 out var hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);

            float currentY = baseTransform.position.y;

            if (isGrounded)
            {
                if (!wasGrounded)
                {
                    // baru landing
                    float dropHeight = falling ? (fallStartY - currentY) : 0f;
                    if (falling)
                    {
                        if (dropHeight >= minFallHeightForFail)
                        {
                            Fail("Microscope.Fail.Drop",
                                $"Percobaan gagal: Mikroskop jatuh dari {dropHeight:0.00} m.");
                        }
                        else if (dropHeight >= minMinorDropToWarn && warnCooldown <= 0f)
                        {
                            Warn("Hati-hati: Mikroskop sempat terjatuh kecil. Pegang dari basis/arm.");
                            warnCooldown = WarnCooldownSeconds;
                        }
                    }
                    falling = false;
                }

                wasGrounded = true;
                lastGroundedY = currentY;
            }
            else
            {
                if (wasGrounded)
                {
                    // mulai jatuh
                    falling = true;
                    fallStartY = lastGroundedY;
                }
                wasGrounded = false;
            }
        }

        // -------- Tilt logic --------
        void TiltLogic()
        {
            if (isDocked) // di dock: toleran
            {
                tiltWarningTimer = 0f;
                tiltFailTimer = 0f;
                return;
            }

            float tiltDeg = Vector3.Angle(transform.up, Vector3.up);

            if (tiltDeg >= tiltWarningDegrees)
            {
                tiltWarningTimer += Time.deltaTime;

                if (tiltDeg >= tiltFailDegrees)
                {
                    tiltFailTimer += Time.deltaTime;
                    if (tiltFailTimer >= tiltFailHold)
                    {
                        Fail("Microscope.Fail.Drop",
                            $"Percobaan gagal: Mikroskop terlalu miring ({tiltDeg:0}°).");
                        tiltFailTimer = 0f;
                        tiltWarningTimer = 0f;
                    }
                }
                else
                {
                    tiltFailTimer = 0f;
                    if (tiltWarningTimer >= tiltWarningHold && warnCooldown <= 0f)
                    {
                        Warn($"Pegang mikroskop tegak (kemiringan {tiltDeg:0}°).");
                        warnCooldown = WarnCooldownSeconds;
                    }
                }
            }
            else
            {
                tiltWarningTimer = 0f;
                tiltFailTimer = 0f;
            }
        }

        // -------- Public control --------
        public void EnableGuard()  => guardEnabled = true;
        public void DisableGuard() => guardEnabled = false;

        // -------- Helper: feedback & quest emit --------
        void Warn(string msg)
        {
            OnWarning?.Invoke(msg);
            Debug.LogWarning("[DropGuard][WARN] " + msg);
            EmitQuest("Microscope.Warn", new { message = msg });
        }

        void Fail(string questKey, string reason)
        {
            GameController.Instance.DisableCharacterController();
            
            OnFail?.Invoke(reason);
            Debug.LogError("[DropGuard][FAIL] " + reason);
            EmitQuest(questKey, new { reason });
            // Subscriber OnFail bisa freeze input / restart step.
        }

        void EmitQuest(string key, object payload)
        {
            try
            {
                var questType = Type.GetType("QuestEvents");
                if (questType != null)
                {
                    var mi = questType.GetMethod("Emit", BindingFlags.Public | BindingFlags.Static, null,
                        new[] { typeof(string), typeof(object) }, null)
                             ?? questType.GetMethod("Emit", BindingFlags.Public | BindingFlags.Static, null,
                        new[] { typeof(string) }, null);

                    if (mi != null)
                    {
                        if (mi.GetParameters().Length == 2) mi.Invoke(null, new[] { key, payload });
                        else mi.Invoke(null, new object[] { key });
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[DropGuard] QuestEvents.Emit reflection failed: " + e.Message);
            }
            Debug.Log($"[DropGuard] Emit {key}");
        }
    }
}
