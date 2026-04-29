using UnityEngine;

public class NavigationArrowController : MonoBehaviour
{
    public NavigationMonitor monitor;
    public GuidedPathRenderer renderer;
    public DirectionArrow arrow;


    [Header("Arrow Lookahead")]
    public int lookAheadCount = 3; // how many waypoints ahead to aim at

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
        if (renderer == null || !renderer.navigationActive)
            return;

        arrow.gameObject.SetActive(true);
        UpdateArrowTarget();
    }

    void HandleBackOnTrack()
    {
        arrow.gameObject.SetActive(false);
    }

    void Update()
    {
        // SAFE CHECK: if navigation stopped, always disable arrow
        if (renderer == null || !renderer.navigationActive)
        {
            if (arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(false);

            return;
        }

        if (!arrow.gameObject.activeSelf) return;

        UpdateArrowTarget();
    }

    void UpdateArrowTarget()
    {
        
            if (renderer.CurrentPath == null || renderer.CurrentPath.Count < 1)
                return;

            int idx = renderer.CurrentNextIndex;
            int count = renderer.CurrentPath.Count;

            Transform player = renderer.player;

            // ---------- NEW LOGIC: LOOK AHEAD N WAYPOINTS ----------
            int targetIndex = Mathf.Min(idx + lookAheadCount, count - 1);

    

            arrow.SetTarget(renderer.CurrentPath[targetIndex].transform);

        

    }
}
