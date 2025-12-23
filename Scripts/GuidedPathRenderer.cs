using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class GuidedPathRenderer : MonoBehaviour // Makes the path visible in the VR world Vizulation
{
    [Header("Path Settings")]
    public Transform player;
    public WaypointPathfinder pathfinder;
    public Waypoint exitWaypoint;

    [Header("Line Appearance")]
    public float lineYOffset = 0.05f;
    public float lineWidth = 0.15f;

    [Header("Scrolling & Orientation")]
    public float scrollSpeed = 2f;
    public float textureScale = 1f;
    public bool invertScrollDirection = false;
    public bool rotateTexture90 = false;
    public bool flipTextureHorizontal = false;
    public bool flipTextureVertical = false;

    private LineRenderer line;
    private Material runtimeMat;
    [Header("Texture Scaling")]
    [Tooltip("Controls how large the arrow appears on the line.")]
    public float arrowSize = 1f;


    public List<Waypoint> CurrentPath { get; private set; }
    public int CurrentNextIndex { get; private set; } = 0;


    public float DistancePublicref;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.widthMultiplier = lineWidth;

        // clone material
        runtimeMat = Instantiate(line.material);
        line.material = runtimeMat;

        ApplyTextureOrientation();
    }

    void Update()
    {
        UpdateScroll();

        Waypoint nearest = GetClosestWaypoint();
        if (nearest == null) return;

        var newPath = pathfinder.GetShortestPath(nearest, exitWaypoint);

        // Only update if path changed
        if (CurrentPath == null || PathChanged(newPath))
        {
            CurrentPath = newPath;
            CurrentNextIndex = 0;
        }

        RenderPath(CurrentPath);

        // REQUIRED FOR NAVIGATION MONITOR TO WORK
        AdvanceWaypointIfReached();
    }

    bool PathChanged(List<Waypoint> newPath)
    {
        if (CurrentPath == null || newPath.Count != CurrentPath.Count)
            return true;

        for (int i = 0; i < newPath.Count; i++)
            if (newPath[i] != CurrentPath[i]) return true;

        return false;
    }


    void ApplyTextureOrientation()
    {
        Vector2 scale = runtimeMat.mainTextureScale;

        // Flip
        scale.x *= flipTextureHorizontal ? -1f : 1f;
        scale.y *= flipTextureVertical ? -1f : 1f;

        // Rotate 90 degrees swap axes
        if (rotateTexture90)
            runtimeMat.mainTextureScale = new Vector2(scale.y, scale.x);
        else
            runtimeMat.mainTextureScale = scale;
    }

    void UpdateScroll()
    {
        if (runtimeMat == null) return;

        float direction = invertScrollDirection ? -1f : 1f;
        float offset = Time.time * scrollSpeed * direction;

        if (rotateTexture90)
            runtimeMat.mainTextureOffset = new Vector2(0, offset);
        else
            runtimeMat.mainTextureOffset = new Vector2(offset, 0);
    }

    void RenderPath(List<Waypoint> path)
    {
        if (path == null || path.Count < 2)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.Count;

        float totalDist = 0;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i].transform.position;
            pos.y += lineYOffset;
            line.SetPosition(i, pos);

            if (i > 0)
                totalDist += Vector3.Distance(path[i].transform.position, path[i - 1].transform.position);

            DistancePublicref = totalDist;
        }

        // dynamic tiling based on path length
        // Dynamic tiling (arrow size control)
        float tileRepeat = Mathf.Max(0.1f, totalDist * textureScale / arrowSize);
        runtimeMat.mainTextureScale = rotateTexture90 ? new Vector2(1, tileRepeat) : new Vector2(tileRepeat, 1);

        ApplyTextureOrientation(); // ensure changes update live

        line.widthMultiplier = lineWidth;
    }

    Waypoint GetClosestWaypoint()
    {
        float best = Mathf.Infinity;
        Waypoint closest = null;

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



    //helper for wrong Direction
    public Vector3 GetNextTargetPosition()
    {
        if (CurrentPath == null || CurrentPath.Count < 2)
            return player.position;

        // Ensure index is valid
        CurrentNextIndex = Mathf.Clamp(CurrentNextIndex, 0, CurrentPath.Count - 1);

        return CurrentPath[CurrentNextIndex].transform.position;
    }

    public void AdvanceWaypointIfReached(float radius = 0.5f)
    {
        if (CurrentPath == null || CurrentNextIndex >= CurrentPath.Count - 1) return;

        float dist = Vector3.Distance(player.position, CurrentPath[CurrentNextIndex].transform.position);

        if (dist < radius)
            CurrentNextIndex++;
    }

    public void ForceRecalculate()
    {
        CurrentPath = null; // clear cached path so Update recomputes next frame
    }


}
