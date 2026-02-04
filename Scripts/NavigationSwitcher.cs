using UnityEngine;

public class NavigationSwitcher : MonoBehaviour
{
    public WaypointPathfinder pathfinder;

    public bool isInSecondary = false;

    public void EnterSecondary()
    {
        isInSecondary = true;
        pathfinder.allowSecondaryWaypoints = true;
    }

    public void ExitSecondary()
    {
        isInSecondary = false;
        pathfinder.allowSecondaryWaypoints = false;
    }
}
