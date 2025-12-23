using UnityEngine;
using System;

public class SecondaryNavigationMonitor : MonoBehaviour
{
    public SecondaryGuidedPathRenderer renderer;
    public float checkInterval = 0.5f;
    public float tolerance = 0.1f;

    public float totalDistance;
    public float currentDistance;
    public bool warningTriggered;

    public event Action OnWrongDirection;
    public event Action OnBackOnTrack;

    float timer;

    void Update()
    {
        if (!renderer || renderer.CurrentPath == null) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0;

        CheckDistanceTrend();
    }

    void CheckDistanceTrend()
    {
        currentDistance = renderer.DistancePublicref;

        if (currentDistance < totalDistance)
        {
            totalDistance = currentDistance;
            if (warningTriggered)
            {
                warningTriggered = false;
                OnBackOnTrack?.Invoke();
            }
            return;
        }

        if (currentDistance > totalDistance + tolerance)
        {
            if (!warningTriggered)
            {
                warningTriggered = true;
                OnWrongDirection?.Invoke();
            }
            totalDistance = currentDistance;
        }
    }
}
