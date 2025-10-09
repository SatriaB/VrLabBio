using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TimedUnityEvent : MonoBehaviour
{
    [Tooltip("Time in seconds before the event is triggered.")]
    public float delay = 1f;

    [Tooltip("Event to invoke after the delay.")]
    public UnityEvent onTimeElapsed;

    [Tooltip("Trigger automatically on Start?")]
    public bool triggerOnStart = true;

    public bool post = false;

    private Coroutine timerCoroutine;

    void Start()
    {
        if (triggerOnStart)
            StartTimer();
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    /// <summary>
    /// Stops the timer before it triggers.
    /// </summary>
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(delay);
        onTimeElapsed?.Invoke();
        timerCoroutine = null;
    }
}
