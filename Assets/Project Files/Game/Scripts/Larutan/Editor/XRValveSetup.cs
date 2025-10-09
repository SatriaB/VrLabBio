using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRValveSetup
{
    [MenuItem("GameObject/XR/Valve", false, 10)]
    public static void CreateValve(MenuCommand menuCommand)
    {
        // Root object
        GameObject root = new GameObject("XRValve");
        GameObjectUtility.SetParentAndAlign(root, menuCommand.context as GameObject);

        // Pivot object (for hinge anchor)
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.SetParent(root.transform, false);

        Rigidbody pivotRb = pivot.AddComponent<Rigidbody>();
        pivotRb.useGravity = false;
        pivotRb.isKinematic = true;

        // Handle object
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(pivot.transform, false);
        handle.transform.localScale = new Vector3(0.2f, 0.2f, 1.0f); // long cube
        handle.transform.localPosition = new Vector3(0, 0, 0.5f);

        Rigidbody rb = handle.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // Add hinge joint to handle
        HingeJoint hinge = handle.AddComponent<HingeJoint>();
        hinge.connectedBody = pivotRb;
        hinge.axis = Vector3.up; // rotates around Y axis by default
        hinge.useLimits = true;

        // XR stuff
        XRGrabInteractable grab = handle.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.trackPosition = false;
        grab.trackRotation = true;

        // Valve script
        XRValve valve = handle.AddComponent<XRValve>();
        valve.minAngle = -180f;
        valve.maxAngle = 180f;

        // Register undo
        Undo.RegisterCreatedObjectUndo(root, "Create XRValve");
        Selection.activeObject = root;
    }
}
