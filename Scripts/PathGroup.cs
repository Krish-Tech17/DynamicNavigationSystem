using UnityEngine;
using System.Collections.Generic;

public class PathGroup : MonoBehaviour  // This script creates the waypoint graph
{
    public WaypointPathfinder pathfinder;
    public List<Waypoint> loopWaypoints;   // waypoints placed anywhere
    public Waypoint exitWaypoint;

    [Header("Graph Settings")]
    public float connectRadius = 2.5f;     // how far waypoints can connect
    public float maxAngle = 75f;           // direction filter
    public int maxNeighbors = 2;           // corridor-like connections

    void Awake()
    {

        // Auto detect waypoint children
        loopWaypoints = new List<Waypoint>(GetComponentsInChildren<Waypoint>());


        BuildGraph();

        //  register in pathfinder
        pathfinder.allWaypoints.Clear();
        pathfinder.allWaypoints.AddRange(loopWaypoints);
        pathfinder.allWaypoints.Add(exitWaypoint);
    }

    void BuildGraph()
    {
        //  clear all neighbors first
        foreach (var wp in loopWaypoints)
            wp.neighbors.Clear();

        exitWaypoint.neighbors.Clear();

        //  build graph automatically (NO ORDER required)
        foreach (var wp in loopWaypoints)
        {
            List<Waypoint> candidates = new List<Waypoint>();

            foreach (var other in loopWaypoints)
            {
                if (wp == other) continue;
                if (other == exitWaypoint) continue;

                float dist = Vector3.Distance(wp.transform.position, other.transform.position);
                if (dist > connectRadius) continue;

                candidates.Add(other);
            }

            //  sort closest first
            candidates.Sort((a, b) =>
                Vector3.Distance(wp.transform.position, a.transform.position)
                .CompareTo(Vector3.Distance(wp.transform.position, b.transform.position)));

            //  determine forward direction
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


                //  very close points always connect
                if (distToC > 1.2f && angle > maxAngle) continue;

                if (!wp.neighbors.Contains(c))
                    wp.neighbors.Add(c);

                if (!c.neighbors.Contains(wp))
                    c.neighbors.Add(wp);

                added++;
            }

        }
        //  EXIT SPECIAL LOGIC (never auto-connect)
        List<Waypoint> sorted = new List<Waypoint>(loopWaypoints);
        sorted.Sort((a, b) =>
            Vector3.Distance(exitWaypoint.transform.position, a.transform.position)
            .CompareTo(Vector3.Distance(exitWaypoint.transform.position, b.transform.position)));

        // Connect exit to nearest valid waypoint
        for (int i = 0; i < Mathf.Min(2, sorted.Count); i++)
        {
            var closestToExit = sorted[i];

            if (!exitWaypoint.neighbors.Contains(closestToExit))
                exitWaypoint.neighbors.Add(closestToExit);

            if (!closestToExit.neighbors.Contains(exitWaypoint))
                closestToExit.neighbors.Add(exitWaypoint);
        }

        // Ensure no waypoint is isolated or stuck with only 1 neighbor
        foreach (var wp in loopWaypoints)
        {
            if (wp.neighbors.Count == 1)
            {
                // find nearest valid extra candidate
                Waypoint nearest = null;
                float bestDist = Mathf.Infinity;

                foreach (var other in loopWaypoints)
                {
                    if (other == wp || wp.neighbors.Contains(other)) continue;

                    float dist = Vector3.Distance(wp.transform.position, other.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
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



        foreach (var wp in loopWaypoints)
        {
            while (wp.neighbors.Count > maxNeighbors)
            {
                wp.neighbors.Sort((a, b) =>
                    Vector3.Distance(wp.transform.position, b.transform.position)
                    .CompareTo(Vector3.Distance(wp.transform.position, a.transform.position)));

                wp.neighbors.RemoveAt(0); // remove farthest
            }

        }

    }

    // to assign and runtime exit or destination
    public void SetNewExit(Waypoint newExit)
    {
        if (newExit == null) return;

        // remove old exit connections
        foreach (var wp in loopWaypoints)
            wp.neighbors.Remove(exitWaypoint);

        exitWaypoint = newExit;

        // rebuild full graph with new exit included
        BuildGraph();

        // update to pathfinder
        pathfinder.allWaypoints.Clear();
        pathfinder.allWaypoints.AddRange(loopWaypoints);
        pathfinder.allWaypoints.Add(exitWaypoint);

        Debug.Log($"Exit changed to: {exitWaypoint.name}");
    }

}
