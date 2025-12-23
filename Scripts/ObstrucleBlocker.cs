using UnityEngine;
using System.Collections.Generic;

public class ObstrucleBlocker : MonoBehaviour
{
    [Header("Settings")]
    public float detectRadius = 1.5f;          // Search range for waypoints
    public int maxAffectedWaypoints = 2;       // Nearest X points to block
    public float delayBeforeScan = 2f;         // Wait time after spawn

    private WaypointPathfinder pathfinder;
    private readonly List<Waypoint> affectedWaypoints = new List<Waypoint>();

    void OnEnable()
    {
        // Auto locate pathfinder in scene if not assigned
        pathfinder = FindFirstObjectByType<WaypointPathfinder>();

        if (pathfinder == null)
        {
            Debug.LogError("SimpleObstacleBlocker: No WaypointPathfinder found in scene.");
            return;
        }

        Invoke(nameof(BlockNearby), delayBeforeScan);
    }

    private void BlockNearby()
    {
        if (pathfinder == null) return;

        affectedWaypoints.Clear();

        List<Waypoint> candidates = new List<Waypoint>();

        // Detect waypoints within radius
        foreach (Waypoint wp in pathfinder.allWaypoints)
        {
            float dist = Vector3.Distance(transform.position, wp.transform.position);

            if (dist <= detectRadius)
                candidates.Add(wp);
        }

        // Sort closest first
        candidates.Sort((a, b) =>
            Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(Vector3.Distance(transform.position, b.transform.position))
        );

        // Pick nearest
        int count = Mathf.Min(maxAffectedWaypoints, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            candidates[i].blocked = true;
            affectedWaypoints.Add(candidates[i]);
        }

        Debug.Log($"Obstacle blocked {affectedWaypoints.Count} waypoint(s).");
    }

    private void OnDestroy()
    {
        Unblock();
    }

    private void OnDisable()
    {
        Unblock();
    }

    private void Unblock()
    {
        foreach (Waypoint wp in affectedWaypoints)
            if (wp != null)
                wp.blocked = false;

        affectedWaypoints.Clear();
    }

    // ---------------------- GIZMOS ----------------------
    private void OnDrawGizmosSelected()
    {
        // Draw radius outline
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // If scene is playing, show affected waypoints
        if (affectedWaypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var wp in affectedWaypoints)
            {
                if (wp != null)
                    Gizmos.DrawLine(transform.position, wp.transform.position);
            }
        }
    }
}
