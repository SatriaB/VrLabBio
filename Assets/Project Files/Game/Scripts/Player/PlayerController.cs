using Unity.XR.CoreUtils;
using UnityEngine;

namespace FatahDev
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private XROrigin xrOrigin;

		[BoxGroup("RayCast", "Raycast")]
		[SerializeField]
		private float maxDistanceRay;

		private Camera mainCamera;

		[SerializeField]
		private Camera mikroskopCamera;

		private bool mikroskopEnabled;

		private void Start()
		{
			mainCamera = xrOrigin.Camera;
		}

		private void Update()
		{
			if (xrOrigin == null) return;

			RaycastHit hit;
			Transform cameraTransform = xrOrigin.Camera.transform;
			bool inMikroskopArea = false;
			if (Physics.Raycast(cameraTransform.position, cameraTransform.forward.normalized, out hit, maxDistanceRay))
			{
				if (hit.collider.CompareTag("Lens"))
				{
					inMikroskopArea = true;

					if (mikroskopEnabled) return;

					mikroskopEnabled = true;

					mainCamera.enabled = !mikroskopEnabled;
					mikroskopCamera.enabled = mikroskopEnabled;
					Debug.Log("In Area Lens");
				}
			}

			if (inMikroskopArea) return;

			if (mikroskopEnabled)
			{
				mikroskopEnabled = false;

				mainCamera.enabled = !mikroskopEnabled;
				mikroskopCamera.enabled = mikroskopEnabled;
			}
		}

		private void OnDrawGizmos()
		{
			if(xrOrigin == null) return;

			Transform cameraTransform = xrOrigin.Camera.transform;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + (cameraTransform.forward * maxDistanceRay));
		}
	}
}
