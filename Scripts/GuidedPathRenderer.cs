using UnityEngine;
using System.Collections;
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
    public bool invertScrollDirection = false;
    public bool rotateTexture90 = false;
    public bool flipTextureHorizontal = false;
    public bool flipTextureVertical = false;

    [Header("Texture Scaling")]
    public float arrowSize = 1f;

    private LineRenderer line;
    private Material runtimeMat;

    public List<Waypoint> CurrentPath { get; private set; }
    public int CurrentNextIndex { get; private set; } = 0;

    public float DistancePublicref;
    public bool navigationActive = false;

    private Waypoint lastNearest;

    [Header("Recalculation")]
    public float nearestCheckInterval = 0.5f;
    private float nearestTimer = 0f;

    // ⭐ delay system
    public float requestDelay = 0.4f;
    private Coroutine pendingRequest;

    // ----------------------------

    public void StartNavigationTo(Waypoint target)
    {
        exitWaypoint = target;
        navigationActive = true;

        lastNearest = GetClosestWaypoint();
        RequestRecalculation();
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
        line.textureMode = LineTextureMode.RepeatPerSegment;

        runtimeMat = Instantiate(line.material);
        line.material = runtimeMat;

        ApplyTextureOrientation();
    }

    void Update()
    {
        if (!navigationActive) return;

        UpdateScroll();

        nearestTimer += Time.deltaTime;

        if (nearestTimer >= nearestCheckInterval)
        {
            nearestTimer = 0f;

            Waypoint nearest = GetClosestWaypoint();

            if (nearest != lastNearest)
            {
                lastNearest = nearest;
                RequestRecalculation();
            }
        }

        AdvanceWaypointIfReached();
    }

    // ---------------------------- SMART REQUEST ----------------------------

    void RequestRecalculation()
    {
        if (pendingRequest != null)
            StopCoroutine(pendingRequest);

        pendingRequest = StartCoroutine(DelayedRecalculation());
    }

    IEnumerator DelayedRecalculation()
    {
        yield return new WaitForSeconds(requestDelay);

        if (!navigationActive || lastNearest == null || exitWaypoint == null)
            yield break;

        // heavy call but happens rarely now
        CurrentPath = pathfinder.GetShortestPath(lastNearest, exitWaypoint);
        CurrentNextIndex = 0;

        RenderPath(CurrentPath);
    }

    // ----------------------------------------------------------------------

    private float scrollOffset = 0f;

    void UpdateScroll()
    {
        if (runtimeMat == null) return;

        float direction = invertScrollDirection ? -1f : 1f;
        scrollOffset += (scrollSpeed / arrowSize) * Time.deltaTime * direction;

        runtimeMat.mainTextureOffset = rotateTexture90
            ? new Vector2(0, scrollOffset)
            : new Vector2(scrollOffset, 0);
    }

    void RenderPath(List<Waypoint> path)
    {
        if (path == null || path.Count < 2)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.Count;

        float totalDist = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i].transform.position;
            pos.y += lineYOffset;
            line.SetPosition(i, pos);

            if (i > 0)
                totalDist += Vector3.Distance(
                    path[i].transform.position,
                    path[i - 1].transform.position
                );

            DistancePublicref = totalDist;
        }

        float tilesPerMeter = 1f / arrowSize;

        runtimeMat.mainTextureScale = rotateTexture90
            ? new Vector2(1f, tilesPerMeter)
            : new Vector2(tilesPerMeter, 1f);

        ApplyTextureOrientation();
        line.widthMultiplier = lineWidth;
    }

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
        lastNearest = GetClosestWaypoint();
        RequestRecalculation();
    }
}
