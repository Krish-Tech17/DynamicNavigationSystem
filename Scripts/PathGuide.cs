using UnityEngine;
using System.Collections.Generic;

public class PathGuide : MonoBehaviour
{
    public Transform player;
    public WaypointPathfinder pathfinder;
    public Waypoint exitWaypoint;
    public LineRenderer line;
    public ArrowPathRenderer arrowRenderer;

    void Update()
    {
        Waypoint nearest = GetClosestWaypoint();
        if (nearest == null) return;

        var path = pathfinder.GetShortestPath(nearest, exitWaypoint);

        DrawPath(path);
    }

    Waypoint GetClosestWaypoint()
    {
        Waypoint closest = null;
        float best = Mathf.Infinity;

        foreach (var wp in pathfinder.allWaypoints)
        {
            if (wp.blocked) continue;

            float d = Vector3.Distance(player.position, wp.transform.position);
            if (d < best)
            {
                best = d;
                closest = wp;
            }
        }
        return closest;
    }

    //void DrawPath(List<Waypoint> path)
    //{
    //    if (path == null || path.Count < 2)
    //    {
    //        line.positionCount = 0;
    //        return;
    //    }

    //    //  thin line
    //    line.startWidth = 0.1f;
    //    line.endWidth = 0.1f;
    //    // or line.widthMultiplier = 0.01f;

    //    line.positionCount = path.Count;

    //    for (int i = 0; i < path.Count; i++)
    //        line.SetPosition(i, path[i].transform.position + Vector3.up * 0.05f);
    //}
    void DrawPath(List<Waypoint> path)
    {
        if (arrowRenderer != null)
            arrowRenderer.RenderPath(path);

        if (path == null || path.Count < 2)
        {
            line.positionCount = 0;
            if (arrowRenderer) arrowRenderer.RenderPath(null);
            return;
        }

        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, path[i].transform.position + Vector3.up * 0.05f);

        if (arrowRenderer)
            arrowRenderer.RenderPath(path);
    }


}
