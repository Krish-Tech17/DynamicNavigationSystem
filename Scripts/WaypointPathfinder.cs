using System.Collections.Generic;
using UnityEngine;

public class WaypointPathfinder : MonoBehaviour  // This script finds the shortest possible route from the user’s nearest waypoint -> exit.
{
    public List<Waypoint> allWaypoints = new List<Waypoint>();

    public List<Waypoint> GetShortestPath(Waypoint start, Waypoint goal)
    {
        var prev = new Dictionary<Waypoint, Waypoint>();
        var dist = new Dictionary<Waypoint, float>();
        var unvisited = new List<Waypoint>(allWaypoints);

        foreach (var wp in allWaypoints)
        {
            dist[wp] = Mathf.Infinity;
            prev[wp] = null;
        }

        dist[start] = 0;

        while (unvisited.Count > 0)
        {
            unvisited.Sort((a, b) => dist[a].CompareTo(dist[b]));
            var current = unvisited[0];
            unvisited.RemoveAt(0);

            if (current == goal)
                break;

            if (current.blocked)
                continue;

            foreach (var n in current.neighbors)
            {
                //  IMPORTANT FIX: ignore neighbors not part of the valid graph
                if (!allWaypoints.Contains(n)) continue;
                if (n.blocked) continue;

                float d = dist[current] + Vector3.Distance(current.transform.position, n.transform.position);
                if (d < dist[n])
                {
                    dist[n] = d;
                    prev[n] = current;
                }
            }
        }

        var path = new List<Waypoint>();
        var temp = goal;

        while (temp != null)
        {
            if (temp.blocked) break;
            path.Insert(0, temp);
            temp = prev[temp];
        }

        return path;
    }
}
