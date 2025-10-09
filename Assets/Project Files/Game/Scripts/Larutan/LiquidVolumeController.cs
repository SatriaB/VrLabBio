using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class FillThresholdEvent
{
    [Range(0f, 1f)]
    public float threshold = 0.5f; // fraction of fill (0 = empty, 1 = full)
    public UnityEvent onThresholdReached;

    [HideInInspector]
    public bool triggered = false; // to prevent multiple triggers
}

public class LiquidVolumeController : MonoBehaviour
{
    [Header("Target Liquid Controller")]
    public LiquidController liquidController; // Your class that has 'float fill'

    [Header("Decrease Settings")]
    public float decreaseSpeed = 0.1f; // fill units per second

    [Header("Threshold Events")]
    public List<FillThresholdEvent> thresholds = new List<FillThresholdEvent>();

    void Update()
    {
        if (liquidController == null)
            return;

        // Decrease fill
        liquidController.fill -= decreaseSpeed * Time.deltaTime;
        liquidController.fill = Mathf.Max(liquidController.fill, 0f);

        // Check thresholds
        foreach (var t in thresholds)
        {
            if (!t.triggered && liquidController.fill <= t.threshold)
            {
                t.triggered = true;
                t.onThresholdReached?.Invoke();
            }
        }
    }

    /// <summary>
    /// Reset thresholds so events can trigger again
    /// </summary>
    public void ResetThresholds()
    {
        foreach (var t in thresholds)
            t.triggered = false;
    }

    /// <summary>
    /// Reset fill to max
    /// </summary>
    public void ResetFill(float value = 1f)
    {
        if (liquidController != null)
        {
            liquidController.fill = value;
            ResetThresholds();
        }
    }
}
