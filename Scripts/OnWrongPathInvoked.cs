using System.Collections;
using UnityEngine;

public class OnWrongPathInvoked : MonoBehaviour
{
    [Header("References")]
    public NavigationMonitor monitor;
    public GameObject warningPanel;   // The red panel UI object

    [Header("Blink Settings")]
    public float blinkDuration = 0.2f;
    public int blinkCount = 2;

    private Coroutine blinkRoutine;

    void Start()
    {
        if (monitor == null)
            monitor = FindFirstObjectByType<NavigationMonitor>();

        if (monitor != null)
        {
            monitor.OnWrongDirection += HandleWrongDirection;
            monitor.OnBackOnTrack += HandleBackOnTrack;
        }

        warningPanel.SetActive(false);
    }

    void HandleWrongDirection()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkWarning());
    }

    void HandleBackOnTrack()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        warningPanel.SetActive(false);
    }

    IEnumerator BlinkWarning()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            warningPanel.SetActive(true);
            yield return new WaitForSeconds(blinkDuration);

            warningPanel.SetActive(false);
            yield return new WaitForSeconds(blinkDuration);
        }

        // Leave it ON after blinking
        warningPanel.SetActive(true);
    }
}
