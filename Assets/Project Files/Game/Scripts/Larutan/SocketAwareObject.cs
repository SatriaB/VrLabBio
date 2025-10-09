using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class SocketAwareObject : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("If true, the object cannot be grabbed by hands once inside a socket.")]
    public bool disableGrabInSocket = true;

    private XRGrabInteractable grab;
    private InteractionLayerMask defaultLayer;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        defaultLayer = grab.interactionLayers;

        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (disableGrabInSocket && args.interactorObject is XRSocketInteractor)
        {
            // Disable hand grabs (clear interaction layer mask)
            grab.interactionLayers = 0;
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (disableGrabInSocket && args.interactorObject is XRSocketInteractor)
        {
            // Restore hand grabs
            grab.interactionLayers = defaultLayer;
        }
    }
}
