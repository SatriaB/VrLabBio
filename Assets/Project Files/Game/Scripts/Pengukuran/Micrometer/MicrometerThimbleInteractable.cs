using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    [DisallowMultipleComponent]
    public class MicrometerThimbleInteractable : XRBaseInteractable
    {
        public enum LocalAxis { X, Y, Z }
        
        [Header("Feel")]
        [SerializeField, Range(0.05f, 2f)] private float rotationSensitivity = 0.40f; // kecil = lebih berat
        [SerializeField] private bool smoothRotation = true;
        [SerializeField, Min(0f)] private float rotationSmoothSpeed = 12f; // smoothing expo
        [SerializeField, Min(0f)] private float handAngleDeadzone = 2f;    // abaikan gerak kecil

        [Header("Rotation")]
        [SerializeField] private LocalAxis rotateAround = LocalAxis.Z;
        [SerializeField] private float minAngle = 0f;    // deg
        [SerializeField] private float maxAngle = 270f;  // deg
        [SerializeField] private bool clampAngle = true;

        [Header("Output")]
        [Range(0,1)] public float value01;
        public UnityEvent<float> onValueChanged;

        [Header("Pose Guard")]
        [SerializeField] private bool lockLocalPosition = true;
        
        [Header("Limiter (opsional)")]
        [SerializeField, Min(0f)] private float maxDegreesPerSecond = 360f; 

        // runtime state
        IXRSelectInteractor interactor;
        Vector3 baseLocalPos;
        Vector3 baseLocalEuler;

        // rotasi referensi saat mulai grab
        Quaternion startKnobWorldRot;
        Vector3    rotAxisWorld;
        Vector3    refVecWorld;     // vRef di bidang ortogonal axis
        public float startLocalAngle; // sudut lokal awal di sumbu yang dipakai
        
        [Header("Slide (opsional)")]
        [SerializeField] private bool enableSlide = false;      // <- hidupkan kalau mau bisa geser
        [SerializeField] private LocalAxis slideAxis = LocalAxis.X;
        [SerializeField] private float slideStart = 0f;         // offset awal (meter)
        [SerializeField] private float slideDistance = 0.005f;  // total jarak geser (meter)
        [SerializeField] private bool clampSlide = true;
        
        Vector3 slideAxisVec;
        
        [SerializeField] private GenericCaptureProvider captureProvider;


        protected override void OnEnable()
        {
            base.OnEnable();
            baseLocalPos   = transform.localPosition;
            baseLocalEuler = transform.localEulerAngles;
            
            slideAxisVec = slideAxis switch
            {
                LocalAxis.X => Vector3.right,
                LocalAxis.Y => Vector3.up,
                _           => Vector3.forward
            };

            selectEntered.AddListener(OnGrab);
            selectExited.AddListener(OnRelease);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(OnGrab);
            selectExited.RemoveListener(OnRelease);
            base.OnDisable();
        }

        void OnGrab(SelectEnterEventArgs args)
        {
            interactor = args.interactorObject;

            baseLocalEuler = transform.localEulerAngles;

            // ★ kalau slide aktif, kunci “origin” posisi di awal grab
            if (enableSlide) baseLocalPos = Vector3.zero;

            rotAxisWorld = AxisWorld(rotateAround);
            refVecWorld  = ReferenceVectorOnPlane(rotAxisWorld, transform.forward);
            
            CaptureRouter.SetActiveProvider(captureProvider);

            startLocalAngle = GetCurrentLocalAngle();
        }

        void OnRelease(SelectExitEventArgs args)
        {
            if (args.interactorObject == interactor) interactor = null;
        }

        void Update()
        {
            if (interactor == null) return;

            // proyeksi tangan di bidang rotasi
            Vector3 vRef = refVecWorld;
            Vector3 vNow = interactor.GetAttachTransform(this).position - transform.position;
            vNow = Vector3.ProjectOnPlane(vNow, rotAxisWorld);
            if (vNow.sqrMagnitude < 1e-6f) vNow = vRef;

            // delta sudut tangan relatif
            float handDelta = Vector3.SignedAngle(vRef, vNow, rotAxisWorld);

            // >>> sensitivitas & deadzone <<<
            if (Mathf.Abs(handDelta) < handAngleDeadzone) handDelta = 0f;
            handDelta *= rotationSensitivity;

            // target absolut di ruang lokal
            float target = startLocalAngle + handDelta;
            if (clampAngle) target = Mathf.Clamp(Norm360(target), minAngle, maxAngle);

            // >>> limiter derajat per detik <<<
            float current = GetCurrentLocalAngle();
            float maxStep = maxDegreesPerSecond * Time.deltaTime;
            float step = Mathf.DeltaAngle(current, target);
            step = Mathf.Clamp(step, -maxStep, maxStep);
            float limitedTarget = current + step;

            // >>> smoothing (expo) <<<
            float finalAngle = smoothRotation
                ? Mathf.LerpAngle(current, limitedTarget, 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime))
                : limitedTarget;

            // apply hanya di sumbu aktif
            ApplyKnobLocalAngle(finalAngle);

            // output 0..1 dari sudut akhir (bukan 'target' mentah)
            float t = Mathf.InverseLerp(minAngle, maxAngle, finalAngle);
            if (!Mathf.Approximately(t, value01))
            {
                value01 = t;
                onValueChanged?.Invoke(value01);
            }
            
            if (enableSlide)
            {
                float slide = slideStart + value01 * slideDistance;
                if (clampSlide)
                {
                    float a = Mathf.Min(slideStart, slideStart + slideDistance);
                    float b = Mathf.Max(slideStart, slideStart + slideDistance);
                    slide = Mathf.Clamp(slide, a, b);
                }
                transform.localPosition = baseLocalPos + slideAxisVec * slide;
            }
        }

        void LateUpdate()
        {
            if (!enableSlide && lockLocalPosition)
                transform.localPosition = Vector3.zero;
            
            Vector3 e = transform.localEulerAngles;
            switch (rotateAround)
            {
                case LocalAxis.X:
                    e.y = baseLocalEuler.y; e.z = baseLocalEuler.z;
                    e.x = Clamp360(e.x, minAngle, maxAngle);
                    break;
                case LocalAxis.Y:
                    e.x = baseLocalEuler.x; e.z = baseLocalEuler.z;
                    e.y = Clamp360(e.y, minAngle, maxAngle);
                    break;
                default:
                    e.x = baseLocalEuler.x; e.y = baseLocalEuler.y;
                    e.z = Clamp360(e.z, minAngle, maxAngle);
                    break;
            }
            transform.localEulerAngles = e;
        }

        // ---- helpers ----
        float GetCurrentLocalAngle()
        {
            Vector3 e = transform.localEulerAngles;
            return rotateAround switch
            {
                LocalAxis.X => e.x,
                LocalAxis.Y => e.y,
                _           => e.z,
            };
        }

        void ApplyKnobLocalAngle(float angle)
        {
            Vector3 e = transform.localEulerAngles;
            switch (rotateAround)
            {
                case LocalAxis.X: e.x = angle; break;
                case LocalAxis.Y: e.y = angle; break;
                default:          e.z = angle; break;
            }
            transform.localEulerAngles = e;
        }

        static float Norm360(float a) { a %= 360f; if (a < 0) a += 360f; return a; }
        static float Clamp360(float a, float min, float max) => Mathf.Clamp(Norm360(a), min, max);

        static Vector3 AxisWorld(LocalAxis ax)
        {
            return ax switch
            {
                LocalAxis.X => Vector3.right,
                LocalAxis.Y => Vector3.up,
                _           => Vector3.forward
            };
        }

        static Vector3 ReferenceVectorOnPlane(Vector3 planeNormal, Vector3 preferred)
        {
            Vector3 v = Vector3.ProjectOnPlane(preferred, planeNormal);
            if (v.sqrMagnitude < 1e-6f) v = Vector3.ProjectOnPlane(Vector3.up, planeNormal);
            if (v.sqrMagnitude < 1e-6f) v = Vector3.ProjectOnPlane(Vector3.right, planeNormal);
            return v.normalized;
        }
    }
}
