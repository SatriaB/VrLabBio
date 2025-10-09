using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[System.Serializable]
public class XRValveCheckpoint
{
    public float targetAngle;
    public float margin; // if set to 0, fallback to 1
    public bool repeatable = true;
    public UnityEvent onCheckpointReached;

    [HideInInspector] public bool triggered;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRValve : MonoBehaviour
{
    public float minAngle = -180f;
    public float maxAngle = 180f;

    public XRValveCheckpoint[] checkpoints;

    private Rigidbody rb;
    private HingeJoint hinge;
    private XRGrabInteractable grab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        grab = GetComponent<XRGrabInteractable>();

        rb.useGravity = false;
        rb.isKinematic = true;

        hinge.useLimits = true;
        JointLimits limits = new JointLimits
        {
            min = minAngle,
            max = maxAngle
        };
        hinge.limits = limits;

        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    private void Update()
    {
        float currentAngle = hinge.angle;
        CheckCheckpoints(currentAngle);
    }

    private void CheckCheckpoints(float currentAngle)
    {
        foreach (var checkpoint in checkpoints)
        {
            // fallback margin
            float margin = checkpoint.margin <= 0f ? 1f : checkpoint.margin;

            bool withinRange = Mathf.Abs(currentAngle - checkpoint.targetAngle) <= margin;

            if (withinRange && (!checkpoint.triggered || checkpoint.repeatable))
            {
                checkpoint.onCheckpointReached?.Invoke();
                checkpoint.triggered = true;
            }
            else if (!withinRange)
            {
                checkpoint.triggered = false; // reset so it can be triggered again
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        rb.isKinematic = false;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        rb.isKinematic = true;
    }

    public void ForceRelease(XRGrabInteractable grabInteractable)
    {
        // If this object is currently held
        if (grabInteractable.isSelected)
        {
            // Get the interactor currently grabbing this object
            var interactor = grabInteractable.firstInteractorSelecting;

            // Get the interaction manager from the interactable
            var manager = grabInteractable.interactionManager;

            if (manager != null && interactor != null)
            {
                manager.SelectExit(interactor, grabInteractable);
            }
        }
    }
}
