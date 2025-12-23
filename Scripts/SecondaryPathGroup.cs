using System.Collections.Generic;
using UnityEngine;

public class SecondaryPathGroup : MonoBehaviour
{
    public WaypointPathfinder secondaryPathfinder;
    public List<Waypoint> secondaryWaypoints = new List<Waypoint>();

    [Header("Graph Settings (same as primary)")]
    public float connectRadius = 2.5f;
    public float maxAngle = 75f;
    public int maxNeighbors = 2;

    [Header("Primary Exit Linking")]
    public List<Waypoint> primaryExitPoints; // assign 1–3 primary nodes closest to secondary

    void Awake()
    {
        CollectSecondaryWaypoints();
        BuildGraph();
        RegisterToPathfinder();
    }

    void CollectSecondaryWaypoints()
    {
        secondaryWaypoints.Clear();
        secondaryWaypoints.AddRange(GetComponentsInChildren<Waypoint>());
    }

    void RegisterToPathfinder()
    {
        if (secondaryPathfinder == null) return;
        secondaryPathfinder.allWaypoints.Clear();
        secondaryPathfinder.allWaypoints.AddRange(secondaryWaypoints);
    }

    // ------------------------------------------------------------
    // BUILD SECONDARY GRAPH (same logic as primary PathGroup)
    // ------------------------------------------------------------

    void BuildGraph()
    {
        // clear all neighbor lists
        foreach (var wp in secondaryWaypoints)
            wp.neighbors.Clear();

        // --------------------------------------
        // Build local connections inside secondary
        // --------------------------------------
        foreach (var wp in secondaryWaypoints)
        {
            List<Waypoint> candidates = new List<Waypoint>();

            foreach (var other in secondaryWaypoints)
            {
                if (wp == other) continue;

                float dist = Vector3.Distance(wp.transform.position, other.transform.position);
                if (dist > connectRadius) continue;

                candidates.Add(other);
            }

            // sort nearest first
            candidates.Sort((a, b) =>
                Vector3.Distance(wp.transform.position, a.transform.position)
                .CompareTo(Vector3.Distance(wp.transform.position, b.transform.position))
            );

            Vector3 forward = Vector3.zero;
            if (candidates.Count > 0)
                forward = (candidates[0].transform.position - wp.transform.position).normalized;

            int added = 0;

            foreach (var c in candidates)
            {
                if (added >= maxNeighbors) break;

                Vector3 dir = (c.transform.position - wp.transform.position).normalized;
                float angle = Vector3.Angle(forward, dir);
                float distToC = Vector3.Distance(wp.transform.position, c.transform.position);

                if (distToC > 1.2f && angle > maxAngle) continue;

                // connect both ways
                if (!wp.neighbors.Contains(c))
                    wp.neighbors.Add(c);

                if (!c.neighbors.Contains(wp))
                    c.neighbors.Add(wp);

                added++;
            }
        }

        // --------------------------------------
        // Ensure no isolated nodes
        // --------------------------------------
        foreach (var wp in secondaryWaypoints)
        {
            if (wp.neighbors.Count == 1)
            {
                Waypoint nearest = null;
                float best = Mathf.Infinity;

                foreach (var other in secondaryWaypoints)
                {
                    if (other == wp || wp.neighbors.Contains(other)) continue;

                    float d = Vector3.Distance(wp.transform.position, other.transform.position);
                    if (d < best)
                    {
                        best = d;
                        nearest = other;
                    }
                }

                if (nearest != null)
                {
                    wp.neighbors.Add(nearest);
                    nearest.neighbors.Add(wp);
                }
            }
        }

        // --------------------------------------
        // Link selected primary exit waypoints
        // --------------------------------------
        if (primaryExitPoints != null)
        {
            foreach (var sec in secondaryWaypoints)
            {
                foreach (var primary in primaryExitPoints)
                {
                    float d = Vector3.Distance(sec.transform.position, primary.transform.position);

                    // OPTIONAL threshold — if they are close enough
                    if (d < connectRadius)
                    {
                        if (!sec.neighbors.Contains(primary))
                            sec.neighbors.Add(primary);

                        if (!primary.neighbors.Contains(sec))
                            primary.neighbors.Add(sec);
                    }
                }
            }
        }

        Debug.Log($"Secondary graph built. Nodes: {secondaryWaypoints.Count}");
    }
}
