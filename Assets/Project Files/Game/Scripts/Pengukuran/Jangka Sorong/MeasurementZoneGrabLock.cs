﻿using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    /// <summary>
    /// Kunci XRGrabInteractable saat disocket ke zona ukur + (opsional) emit sinyal quest.
    /// Cocok untuk Caliper/Micrometer: step "place" akan complete saat alat disocket.
    /// </summary>
    public class MeasurementZoneGrabLock : MonoBehaviour
    {
        [Header("Socket")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor measureZoneSocket;

        [Tooltip("Layer mask 'aman' yang TIDAK dibaca oleh interactor player (mis. 'Locked').")]
        [SerializeField] private InteractionLayerMask lockedMask;

        // ====== Quest Bridge (tambahan) ======
        public enum InstrumentKind { None, Caliper, Micrometer }

        [Header("Quest Bridge")]
        [Tooltip("Jenis alat yang diparkir di zona ini (untuk auto-mapping sinyal).")]
        [SerializeField] private InstrumentKind instrumentKind = InstrumentKind.None;

        [Tooltip("Emit sinyal quest saat alat masuk/tersocket (disarankan ON).")]
        [SerializeField] private bool emitSignalOnEnter = true;

        [Tooltip("Emit sinyal saat alat keluar/unsocket (opsional).")]
        [SerializeField] private bool emitSignalOnExit = false;

        [Tooltip("Override sinyal EXIT (opsional).")]
        [SerializeField] private string overrideExitSignal = "";

        // ====== State ======
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
        private InteractionLayerMask _originalMask;

        private void Reset()
        {
            measureZoneSocket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (measureZoneSocket == null) return;
            measureZoneSocket.selectEntered.AddListener(OnEnter);
            measureZoneSocket.selectExited.AddListener(OnExit);

            // biar socket nggak gampang lepas
            measureZoneSocket.keepSelectedTargetValid = true;
        }

        private void OnDisable()
        {
            if (measureZoneSocket == null) return;
            measureZoneSocket.selectEntered.RemoveListener(OnEnter);
            measureZoneSocket.selectExited.RemoveListener(OnExit);
        }

        private void OnEnter(SelectEnterEventArgs args)
        {
            _grab = args.interactableObject.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (_grab != null)
            {
                _originalMask = _grab.interactionLayers;
                _grab.interactionLayers = lockedMask; // kunci: interactor player tidak bisa select
                _grab.throwOnDetach = false;
            }

            // === Emit sinyal QUEST untuk step "place"
            if (emitSignalOnEnter)
            {
                var sig = AutoEnterSignal();
                if (!string.IsNullOrEmpty(sig))
                    QuestEvents.Emit(sig);
            }
        }

        private void OnExit(SelectExitEventArgs args)
        {
            if (_grab != null)
            {
                _grab.interactionLayers = _originalMask; // pulihkan
                _grab = null;
            }

            if (emitSignalOnExit)
            {
                var sig = !string.IsNullOrEmpty(overrideExitSignal) ? overrideExitSignal : AutoExitSignal();
                if (!string.IsNullOrEmpty(sig))
                    QuestEvents.Emit(sig);
            }
        }

        // ====== Mapping otomatis sinyal ENTER/EXIT ======
        private string AutoEnterSignal()
        {
            switch (instrumentKind)
            {
                case InstrumentKind.Caliper:
                    // step "place" Caliper
                    return QuestSignals.CALIPER_SPECIMEN_PLACED;
                case InstrumentKind.Micrometer:
                    // step "place" Micrometer
                    return QuestSignals.MICROMETER_SPECIMEN_PLACED;
                default:
                    return null;
            }
        }

        private string AutoExitSignal()
        {
            // Kalau kamu mau beda sinyal saat dilepas, isi di overrideExitSignal.
            // Secara default tidak emit apa-apa saat keluar.
            return null;
        }
    }
}
