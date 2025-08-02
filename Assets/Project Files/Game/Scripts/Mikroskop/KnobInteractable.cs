using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace FatahDev
{
    public class KnobInteractable : XRBaseInteractable
    {
		[SerializeField]
		private Transform knobTransform;

		private IXRSelectInteractor pullingInteractor;

		public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
		{
			base.ProcessInteractable(updatePhase);

			if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
			{
				if (pullingInteractor != null)
				{
					DistanceInfo info = GetDistance(knobTransform.position);
					Debug.Log(info.distanceSqr);
					Transform pullTransform = pullingInteractor.GetAttachTransform(this).transform;
					Quaternion pullRotation =  pullTransform.rotation;

					knobTransform.Rotate(new Vector3(0.0f, pullRotation.y, 0.0f));
				}
			}
		}

		protected override void OnSelectEntered(SelectEnterEventArgs args)
		{
			base.OnSelectEntered(args);

			Debug.Log(args.interactorObject);
			pullingInteractor = args.interactorObject;
		}

		protected override void OnSelectExited(SelectExitEventArgs args)
		{
			base.OnSelectExited(args);
			pullingInteractor = null;
		}
	}
}
