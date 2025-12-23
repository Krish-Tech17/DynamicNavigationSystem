using UnityEngine;

public class NavigationArrowController : MonoBehaviour
{
    public NavigationMonitor monitor;
    public GuidedPathRenderer renderer;
    public DirectionArrow arrow;

    void Start()
    {
        if (!monitor) monitor = FindAnyObjectByType<NavigationMonitor>();
        if (!renderer) renderer = FindAnyObjectByType<GuidedPathRenderer>();

        arrow.gameObject.SetActive(false);

        monitor.OnWrongDirection += HandleWrongDirection;
        monitor.OnBackOnTrack += HandleBackOnTrack;
    }

    void HandleWrongDirection()
    {
        arrow.gameObject.SetActive(true);
        UpdateArrowTarget();
    }

    void HandleBackOnTrack()
    {
        arrow.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!arrow.gameObject.activeSelf) return;
        UpdateArrowTarget();  // VR cheap update
    }

    void UpdateArrowTarget()
    {
        if (renderer.CurrentPath == null || renderer.CurrentPath.Count < 1)
            return;

        int idx = renderer.CurrentNextIndex;
        int count = renderer.CurrentPath.Count;

        Transform player = renderer.player;
        Transform wpCurrent = renderer.CurrentPath[idx].transform;

        float dist = Vector3.Distance(player.position, wpCurrent.position);

        // ⭐ LOOKAHEAD #1 — if close to current waypoint → use next waypoint
        if (dist < 1.0f && idx < count - 1)
        {
            idx++;
        }

        // ⭐ LOOKAHEAD #2 — if VERY close → use next+1 waypoint
        if (idx < count - 2)
        {
            Transform wpNext = renderer.CurrentPath[idx].transform;
            float dist2 = Vector3.Distance(player.position, wpNext.position);

            if (dist2 < 0.5f)
                idx++;
        }

        // ⭐ Assign final target
        arrow.SetTarget(renderer.CurrentPath[idx].transform);
    }
}
