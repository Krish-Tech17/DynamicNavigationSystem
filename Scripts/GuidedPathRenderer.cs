using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(LineRenderer))]
public class GuidedPathRenderer : MonoBehaviour
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

    [Header("Texture Scaling")]
    [Tooltip("World size of one arrow (meters)")]
    public float arrowSize = 1f;

    [Header("Corner Fix")]
    public float cornerOffset = 0.25f;

    private LineRenderer line;
    private Material runtimeMat;
    private float cachedPathLength = 1f;

    public List<Waypoint> CurrentPath { get; private set; }
    public int CurrentNextIndex { get; private set; } = 0;

    public float DistancePublicref;
    public bool navigationActive = false;

    public float recalcInterval = 4.0f;
    private float recalcTimer = 0f;
    private Waypoint lastNearest;

    private float scrollOffset = 0f;

    // ----------------------------

    public void StartNavigationTo(Waypoint target)
    {
        exitWaypoint = target;
        navigationActive = true;
        RecalculatePath();
    }

    public void StopNavigation()
    {
        navigationActive = false;
        line.positionCount = 0;
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.widthMultiplier = lineWidth;

        line.textureMode = LineTextureMode.Tile;

        runtimeMat = Instantiate(line.material);
        line.material = runtimeMat;

        ApplyTextureOrientation();
    }

    void Update()
    {
        if (!navigationActive) return;

        UpdateScroll();

        recalcTimer += Time.deltaTime;
        Waypoint nearest = GetClosestWaypoint();

        if (nearest != lastNearest || recalcTimer > recalcInterval)
        {
            RecalculatePath();
            lastNearest = nearest;
            recalcTimer = 0f;
            RenderPath(CurrentPath);
        }

        AdvanceWaypointIfReached();
    }

    public void RecalculatePath()
    {
        Waypoint nearest = GetClosestWaypoint();
        if (nearest == null || exitWaypoint == null) return;

        CurrentPath = pathfinder.GetShortestPath(nearest, exitWaypoint);
        CurrentNextIndex = 0;
    }

    // ---------------------------- SCROLL ----------------------------

    void UpdateScroll()
    {
        if (runtimeMat == null) return;

        float direction = invertScrollDirection ? -1f : 1f;

        scrollOffset += (scrollSpeed / arrowSize) * Time.deltaTime * direction;

        runtimeMat.mainTextureOffset = rotateTexture90
            ? new Vector2(0, scrollOffset)
            : new Vector2(scrollOffset, 0);
    }

    // ---------------------------- CORNER FIX ----------------------------

    List<Vector3> BuildCornerFixedPath(List<Waypoint> path)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i].transform.position;

            if (i > 0 && i < path.Count - 1)
            {
                Vector3 prev = path[i - 1].transform.position;
                Vector3 next = path[i + 1].transform.position;

                Vector3 dir1 = (pos - prev).normalized;
                Vector3 dir2 = (next - pos).normalized;

                float angle = Vector3.Angle(dir1, dir2);

                if (angle > 10f)
                {
                    Vector3 p1 = pos - dir1 * cornerOffset;
                    Vector3 p2 = pos + dir2 * cornerOffset;

                    result.Add(p1);
                    result.Add(p2);
                    continue;
                }
            }

            result.Add(pos);
        }

        return result;
    }

    // ----------------------------

    void RenderPath(List<Waypoint> path)
    {
        if (path == null || path.Count < 2)
        {
            line.positionCount = 0;
            return;
        }

        List<Vector3> positions = BuildCornerFixedPath(path);

        line.positionCount = positions.Count;

        float totalDist = 0f;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            pos.y += lineYOffset;
            line.SetPosition(i, pos);

            if (i > 0)
                totalDist += Vector3.Distance(
                    positions[i],
                    positions[i - 1]
                );
        }

        DistancePublicref = totalDist;

        cachedPathLength = Mathf.Max(0.01f, totalDist);

        float tilesPerMeter = 1f / arrowSize;

        runtimeMat.mainTextureScale = rotateTexture90
            ? new Vector2(1f, tilesPerMeter)
            : new Vector2(tilesPerMeter, 1f);

        ApplyTextureOrientation();

        line.widthMultiplier = lineWidth;
    }

    // ----------------------------

    void ApplyTextureOrientation()
    {
        Vector2 scale = runtimeMat.mainTextureScale;

        scale.x *= flipTextureHorizontal ? -1f : 1f;
        scale.y *= flipTextureVertical ? -1f : 1f;

        runtimeMat.mainTextureScale = rotateTexture90
            ? new Vector2(scale.y, scale.x)
            : scale;
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

    public Vector3 GetNextTargetPosition()
    {
        if (CurrentPath == null || CurrentPath.Count < 2)
            return player.position;

        CurrentNextIndex = Mathf.Clamp(CurrentNextIndex, 0, CurrentPath.Count - 1);
        return CurrentPath[CurrentNextIndex].transform.position;
    }

    public void AdvanceWaypointIfReached(float radius = 0.5f)
    {
        if (CurrentPath == null || CurrentNextIndex >= CurrentPath.Count - 1) return;

        float dist = Vector3.Distance(
            player.position,
            CurrentPath[CurrentNextIndex].transform.position
        );

        if (dist < radius)
            CurrentNextIndex++;
    }

    public void ForceRecalculate()
    {
        CurrentPath = null;
    }
}