using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace FatahDev
{
    public class SocketDetailUIController : MonoBehaviour
    {
        [Header("Socket")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor stationSocket;

        [Header("Canvas di Socket (buat manual di editor)")]
        [SerializeField] private Canvas detailCanvasOnSocket;

        private void Reset()
        {
            stationSocket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (stationSocket == null) return;
            stationSocket.selectEntered.AddListener(OnEnter);
            stationSocket.selectExited.AddListener(OnExit);
            SetCanvasActive(false);
        }

        private void OnDisable()
        {
            if (stationSocket == null) return;
            stationSocket.selectEntered.RemoveListener(OnEnter);
            stationSocket.selectExited.RemoveListener(OnExit);
        }

        private void OnEnter(SelectEnterEventArgs _)
        {
            SetCanvasActive(true);
        }

        private void OnExit(SelectExitEventArgs _)
        {
            SetCanvasActive(false);
        }

        private void SetCanvasActive(bool active)
        {
            if (detailCanvasOnSocket != null)
                detailCanvasOnSocket.enabled = active;
        }
    }
}