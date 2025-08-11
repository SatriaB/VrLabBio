using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    public class MikroskopLensBehaviour : XRBaseInteractable
    {
        public Vector3 rotationAxis = Vector3.up;
        public float stepAngle = 90f;
        public int totalSteps = 4;
        public float stepThreshold = 25f;
        public float rotationSpeed = 200f; // derajat per detik

        private XRBaseInteractor interactor;
        private Vector3 initialDirection;
        private int currentStep = 0;
        private bool isStepping = false;

        private float accumulatedRotation = 0f;
        private float targetStepRotation = 0f;
        private bool isRotating = false;
        
        public ObjectiveLensProfile[] lensProfiles; // urutan: 4x,10x,40x,100x
        public MicroscopeOpticsController optics;

        private void Start()
        {
            if (lensProfiles == null || lensProfiles.Length == 0 || optics == null) return;
            
            optics.Apply(lensProfiles[0]);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
           OnGrab(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            OnRelease(args);
        }

        private void OnGrab(SelectEnterEventArgs args)
        {
            interactor = args.interactorObject as XRBaseInteractor;
            if (interactor != null)
            {
                Vector3 dir = interactor.transform.position - transform.position;
                initialDirection = ProjectDirectionOnPlane(dir, rotationAxis).normalized;
                isStepping = false;
            }
        }

        private void OnRelease(SelectExitEventArgs args)
        {
            interactor = null;
        }

        private void Update()
        {
            HandleInteraction();
            AnimateRotation();
        }

        private void HandleInteraction()
        {
            if (interactor == null || isRotating)
                return;

            Vector3 dir = interactor.transform.position - transform.position;
            Vector3 currentDirection = ProjectDirectionOnPlane(dir, rotationAxis).normalized;

            if (currentDirection.sqrMagnitude < 0.001f || initialDirection.sqrMagnitude < 0.001f)
                return;

            float angleDelta = Vector3.SignedAngle(initialDirection, currentDirection, rotationAxis);

            if (!isStepping && Mathf.Abs(angleDelta) > stepThreshold)
            {
                int direction = angleDelta > 0 ? 1 : -1;
                currentStep = (currentStep + direction + totalSteps) % totalSteps;

                // Set target rotasi
                targetStepRotation = direction * stepAngle;
                accumulatedRotation = 0f;
                isRotating = true;

                Debug.Log($"Step ke {currentStep}");

                OnStepChanged(currentStep);

                // Reset input sampai tangan balik lagi
                isStepping = true;
            }
            else if (Mathf.Abs(angleDelta) < stepThreshold * 0.5f)
            {
                isStepping = false;
            }
        }

        private void AnimateRotation()
        {
            if (!isRotating)
                return;

            float deltaRotation = rotationSpeed * Time.deltaTime;
            float remaining = Mathf.Abs(targetStepRotation) - Mathf.Abs(accumulatedRotation);
            float rotateThisFrame = Mathf.Min(deltaRotation, remaining) * Mathf.Sign(targetStepRotation);

            transform.Rotate(rotationAxis, rotateThisFrame, Space.Self);
            accumulatedRotation += rotateThisFrame;

            if (Mathf.Abs(accumulatedRotation) >= Mathf.Abs(targetStepRotation))
            {
                isRotating = false;
                accumulatedRotation = 0f;
                targetStepRotation = 0f;
            }
        }

        private Vector3 ProjectDirectionOnPlane(Vector3 vector, Vector3 normal)
        {
            return Vector3.ProjectOnPlane(vector, normal);
        }

        private void OnStepChanged(int stepIndex)
        {
            if (lensProfiles == null || lensProfiles.Length == 0 || optics == null) return;

            // Map step ke profil (0=4x, 1=10x, 2=40x, 3=100x)
            int idx = Mathf.Clamp(stepIndex % lensProfiles.Length, 0, lensProfiles.Length - 1);

            optics.Apply(lensProfiles[idx]);
            Debug.Log($"[Microscope] Active lens: {lensProfiles[idx].displayName}");
        }
    }
}
