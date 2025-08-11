using UnityEngine;

namespace FatahDev
{
    public class MikroskopBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform teleskopTransform;

        public Vector3 axisRotate = Vector3.up;
        public float speedRotate = 100f;
        public float snapAngle = 15f;
        public bool snapOnRelease = true;

        private bool isDragging = false;
        private Vector3 lastMousePos;
        private float totalRotation = 0f;

        private void Update()
        {
            return;
            
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOver()) 
                {
                    isDragging = true;
                    lastMousePos = Input.mousePosition;
                }
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                float angle = -delta.x * speedRotate;
                teleskopTransform.Rotate(axisRotate, angle, Space.Self);
                totalRotation += angle;
                lastMousePos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                if (snapOnRelease)
                {
                    float snapped = Mathf.Round(totalRotation / snapAngle) * snapAngle;
                    float snapOffset = snapped - totalRotation;

                    teleskopTransform.Rotate(axisRotate, snapOffset, Space.Self);
                    totalRotation = snapped;
                }

                isDragging = false;
            }
        }

        private bool IsPointerOver()
        {
            // Optional: gunakan raycast atau collider check
            return true;
        }
    }
}