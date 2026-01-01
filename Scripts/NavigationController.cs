using UnityEngine;

public class NavigationController : MonoBehaviour
{
    public GuidedPathRenderer navigationRenderer;
    public NavigationMonitor monitor;
    public PathGroup pathGroup;   // <-- Assign in inspector!

    void Start()
    {
        navigationRenderer.navigationActive = false;
    }

    public void StartNavigationTo(GameObject destination)
    {
        Waypoint wp = destination.GetComponent<Waypoint>();
        if (wp == null) { Debug.LogError("Destination has no waypoint"); return; }

        pathGroup.SetNewExit(wp);         // <--- MOST IMPORTANT FIX
        navigationRenderer.StartNavigationTo(wp);
        monitor.InitializeTracking();

        Debug.Log("Navigation Started to: " + wp.name);
    }

    public void StopNavigation()
    {
        navigationRenderer.StopNavigation();
        Debug.Log("Navigation Ended");
    }
}
