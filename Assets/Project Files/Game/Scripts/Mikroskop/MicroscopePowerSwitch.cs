using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
    public class MicroscopePowerSwitch : MonoBehaviour
    {
        [Header("External")]
        [SerializeField] private PlayerController playerController; // drag PlayerController di sini

        [Header("Visual Handle")]
        [SerializeField] private Transform handleTransform;         // drag knob/lever mesh
        [SerializeField] private Vector3 localEulerOn  = new Vector3(-25f, 0f, 0f);
        [SerializeField] private Vector3 localEulerOff = new Vector3( 25f, 0f, 0f);
        [SerializeField] private float animateDuration = 0.10f;

        [Header("State")]
        [SerializeField] private bool isOnDefault = false;          // default OFF
        public UnityEvent<bool> onPowerChanged;                     // opsional: hook ke Audio/Haptic/Light

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
        private bool isOn;
        private Coroutine animateRoutine;

        private void Reset()
        {
            handleTransform = transform;
        }

        private void Awake()
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            interactable.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;

            interactable.selectEntered.AddListener(_ => Toggle());
        }

        private void OnDestroy()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(_ => Toggle());
            }
        }

        private void Start()
        {
            isOn = isOnDefault;          // default OFF
            ApplyVisual(isOn, true);

            if (playerController != null)
                playerController.SetMicroscopePower(isOn); // sinkronkan ke sistemmu

            onPowerChanged?.Invoke(isOn);
        }

        public void Toggle() => Set(!isOn);

        public void Set(bool on)
        {
            if (isOn == on) return;
            isOn = on;

            if (isOn)
            {
                QuestEvents.Emit(QuestSignals.MICROSCOPE_ON);
            }

            ApplyVisual(isOn, false);

            if (playerController != null)
                playerController.SetMicroscopePower(isOn);

            onPowerChanged?.Invoke(isOn);
        }

        private void ApplyVisual(bool on, bool instant)
        {
            if (handleTransform == null) return;

            if (animateRoutine != null) StopCoroutine(animateRoutine);

            if (instant || animateDuration <= 0f)
            {
                handleTransform.localEulerAngles = on ? localEulerOn : localEulerOff;
            }
            else
            {
                animateRoutine = StartCoroutine(AnimateHandle(on ? localEulerOn : localEulerOff));
            }
        }

        private System.Collections.IEnumerator AnimateHandle(Vector3 targetEuler)
        {
            Vector3 start = handleTransform.localEulerAngles;
            float t = 0f;
            while (t < animateDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / animateDuration);
                // smootherstep
                u = u * u * u * (u * (u * 6f - 15f) + 10f);
                handleTransform.localEulerAngles = Vector3.LerpUnclamped(start, targetEuler, u);
                yield return null;
            }
            handleTransform.localEulerAngles = targetEuler;
        }

        // Opsional untuk dipanggil via UnityEvent
        public void TurnOn()  => Set(true);
        public void TurnOff() => Set(false);
    }
}
