using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace FatahDev
{
    public class PinsetAnimator : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private XRGrabInteractable grabInteractable;
        [SerializeField] private Animator animator;

        [Header("Animator")]
        [SerializeField] private string isClosedParam = "IsClosed";
        [SerializeField] private bool requireGrabToActivate = true;

        private void Awake()
        {
            if (!grabInteractable) grabInteractable = GetComponent<XRGrabInteractable>();
            if (!animator) animator = GetComponent<Animator>();
        }

        public void OnActivated()
        {
            if (requireGrabToActivate && (grabInteractable == null || !grabInteractable.isSelected))
                return;

            if (animator)
                animator.SetBool(isClosedParam, true);
        }

        public void OnDeactivated()
        {
            if (requireGrabToActivate && (grabInteractable == null || !grabInteractable.isSelected))
                return;

            if (animator)
                animator.SetBool(isClosedParam, false);
        }
    }
}
