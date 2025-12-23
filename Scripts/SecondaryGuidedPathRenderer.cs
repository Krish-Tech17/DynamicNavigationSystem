using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class SecondaryGuidedPathRenderer : MonoBehaviour
{
    public Transform player;
    public WaypointPathfinder secondaryPathfinder;
    public List<Waypoint> primaryExitPoints;

    public float lineYOffset = 0.05f;
    public float lineWidth = 0.15f;

    [Header("Arrow Animation")]
    public float scrollSpeed = 2f;
    public float textureScale = 1f;
    public float arrowSize = 1f;
    public bool invertScrollDirection = false;
    public bool rotateTexture90 = false;
    public bool flipTextureHorizontal = false;
    public bool flipTextureVertical = false;

    public List<Waypoint> CurrentPath { get; private set; }
    public int CurrentNextIndex { get; private set; }

    public float DistancePublicref;

    private LineRenderer line;
    private Material runtimeMat;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;

        // Clone material like primary does
        runtimeMat = Instantiate(line.material);
        line.material = runtimeMat;

        ApplyTextureOrientation();
    }

    void Update()
    {
        UpdateScroll();

        Waypoint nearestSecondary = GetClosestSecondaryWaypoint();
        if (nearestSecondary == null) return;

        Waypoint nearestPrimaryExit = GetNearestPrimaryExit(nearestSecondary);
        if (nearestPrimaryExit == null) return;

        var newPath = secondaryPathfinder.GetShortestPath(nearestSecondary, nearestPrimaryExit);

        CurrentPath = newPath;
        RenderPath(CurrentPath);
        AdvanceWaypointIfReached();
    }

    void UpdateScroll()
    {
        if (!runtimeMat) return;

        float direction = invertScrollDirection ? -1f : 1f;
        float offset = Time.time * scrollSpeed * direction;

        if (rotateTexture90)
            runtimeMat.mainTextureOffset = new Vector2(0, offset);
        else
            runtimeMat.mainTextureOffset = new Vector2(offset, 0);
    }

    void ApplyTextureOrientation()
    {
        Vector2 scale = runtimeMat.mainTextureScale;

        scale.x *= flipTextureHorizontal ? -1f : 1f;
        scale.y *= flipTextureVertical ? -1f : 1f;

        if (rotateTexture90)
            runtimeMat.mainTextureScale = new Vector2(scale.y, scale.x);
        else
            runtimeMat.mainTextureScale = scale;
    }

    Waypoint GetClosestSecondaryWaypoint()
    {
        float best = Mathf.Infinity;
        Waypoint closest = null;

        foreach (var wp in secondaryPathfinder.allWaypoints)
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

    Waypoint GetNearestPrimaryExit(Waypoint from)
    {
        float best = Mathf.Infinity;
        Waypoint bestExit = null;

        foreach (var exit in primaryExitPoints)
        {
            float d = Vector3.Distance(from.transform.position, exit.transform.position);
            if (d < best)
            {
                best = d;
                bestExit = exit;
            }
        }
        return bestExit;
    }

    void RenderPath(List<Waypoint> path)
    {
        if (path == null || path.Count < 2)
        {
            line.positionCount = 0;
            return;
        }

        float totalDist = 0f;
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i].transform.position;
            pos.y += lineYOffset;
            line.SetPosition(i, pos);

            if (i > 0)
                totalDist += Vector3.Distance(path[i].transform.position, path[i - 1].transform.position);
        }

        DistancePublicref = totalDist;

        // --- Add primary-like tiling ---
        float tileRepeat = Mathf.Max(0.1f, totalDist * textureScale / arrowSize);
        runtimeMat.mainTextureScale = rotateTexture90
            ? new Vector2(1, tileRepeat)
            : new Vector2(tileRepeat, 1);
    }

    public void AdvanceWaypointIfReached(float radius = 0.5f)
    {
        if (CurrentPath == null || CurrentNextIndex >= CurrentPath.Count - 1)
            return;

        float dist = Vector3.Distance(player.position, CurrentPath[CurrentNextIndex].transform.position);
        if (dist < radius)
            CurrentNextIndex++;
    }
}
