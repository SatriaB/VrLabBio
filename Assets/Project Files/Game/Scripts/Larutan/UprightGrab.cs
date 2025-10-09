using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class AlwaysUpright : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Rigidbody rb;
    private Vector3 baseEuler;
    private bool isGrabbed = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // store prefab's starting X/Z
        baseEuler = transform.rotation.eulerAngles;

        // prevent physics rotation on X/Z
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        grab.selectEntered.AddListener(_ => isGrabbed = true);
        grab.selectExited.AddListener(_ => isGrabbed = false);
    }

    void Update()
    {
        // Stop physics spinning on Y too
        if(isGrabbed)
        rb.angularVelocity = Vector3.zero;

        // Always force upright rotation (keep prefab X/Z, allow Y spin)
        Vector3 pos = transform.position;
        float yRot = transform.eulerAngles.y;
        transform.SetPositionAndRotation(pos, Quaternion.Euler(baseEuler.x, yRot, baseEuler.z));
    }
}
