using UnityEngine;
using TMPro;
using FatahDev;

public class AnglePercentDisplay : MonoBehaviour
{
    public MicrometerThimbleInteractable useThimble;
    [Header("Target Settings")]
    public Transform target;
    public Axis axis = Axis.Y;

    [Header("Angle Range")]
    public float minAngle = 0f;
    public float maxAngle = 270f;

    [Header("UI")]
    public TextMeshProUGUI displayText;
    public string format = "F0";

    public enum Axis { X, Y, Z }

    private float previousRawAngle;
    private float accumulatedAngle;

    void Start()
    {
        if (!target) target = transform;
        previousRawAngle = GetLocalAngle();
        accumulatedAngle = previousRawAngle;
    }

    void Update()
    {
        if (!target || !displayText) return;

        float currentRawAngle = GetLocalAngle();

        // Difference accounting for wraparound 0–360
        float delta = Mathf.DeltaAngle(previousRawAngle, currentRawAngle);
        accumulatedAngle += delta; // continuous angle tracking
        previousRawAngle = currentRawAngle;

        // Map to % between min and max
        float percent = Mathf.InverseLerp(minAngle, maxAngle, accumulatedAngle) * 100f;
        
        percent = Mathf.Clamp(useThimble ? useThimble.value01 * 100 : percent, 0f, 100f);

        displayText.text = percent.ToString(format) + "%";

        // Optional: Debug info
        // Debug.Log($"Raw: {currentRawAngle:F2}  |  Continuous: {accumulatedAngle:F2}  |  {percent:F1}%");
    }

    float GetLocalAngle()
    {
        Vector3 euler = target.localEulerAngles;
        return axis switch
        {
            Axis.X => euler.x,
            Axis.Y => euler.y,
            Axis.Z => euler.z,
            _ => 0f
        };
    }
}
