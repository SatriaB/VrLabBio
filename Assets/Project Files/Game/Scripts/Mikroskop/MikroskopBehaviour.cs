using UnityEngine;

namespace FatahDev
{
    public class MikroskopBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Transform teleskopTransform;

        [SerializeField]
        private float speedRotate;

        [SerializeField]
        private Vector3 axisRotate;

		private bool rotate = false;

		private Quaternion[] snappedVectors;

		private void Start()
		{
			snappedVectors = new Quaternion[]
			{
				new Quaternion(-17.279f, 0.0f, 0.0f, 0.0f),
				new Quaternion(0.0f, 90.0f, -17.279f, 0.0f),
				new Quaternion(17.279f, 180, 0.0f, 0.0f),
				new Quaternion(0.0f, 270.0f, 17.279f, 0.0f)
			};
		}

		private void Update()
		{
            if (Input.GetMouseButtonDown(0))
            {
				rotate = true;
			}

			if (rotate)
			{
				teleskopTransform.Rotate(axisRotate, Time.deltaTime * speedRotate);
				teleskopTransform.rotation = Quaternion.Euler(teleskopTransform.rotation.eulerAngles.x, teleskopTransform.rotation.eulerAngles.y, teleskopTransform.rotation.eulerAngles.z);
			}

			if (Input.GetMouseButtonUp(0))
            {
				//teleskopTransform.DOLocalRotate(SnappedVector(), 0.5f).SetEasing(Ease.Type.ExpoOut);
				rotate = false;
			}
		}

		private Quaternion SnappedVector()
		{
			Quaternion endValue;
			float currentY = Mathf.Ceil(teleskopTransform.rotation.eulerAngles.y);

			endValue = currentY switch
			{
				>= 0 and 90 => snappedVectors[0],
				>= 91 and 180 => snappedVectors[1],
				>= 181 and 270 => snappedVectors[2],
				_ => snappedVectors[3],
			};

			return endValue;
		}
	}
}
