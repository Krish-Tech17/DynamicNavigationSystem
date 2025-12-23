using UnityEngine;

public class SecondaryNavigationArrowController : MonoBehaviour
{
    public SecondaryNavigationMonitor monitor;
    public SecondaryGuidedPathRenderer renderer;
    public DirectionArrow arrow;

    void Start()
    {
        arrow.gameObject.SetActive(false);

        monitor.OnWrongDirection += ShowArrow;
        monitor.OnBackOnTrack += HideArrow;
    }

    void ShowArrow() => arrow.gameObject.SetActive(true);
    void HideArrow() => arrow.gameObject.SetActive(false);

    void Update()
    {
        if (!arrow.gameObject.activeSelf) return;

        if (renderer.CurrentPath == null) return;

        int idx = renderer.CurrentNextIndex;
        if (idx < renderer.CurrentPath.Count)
            arrow.SetTarget(renderer.CurrentPath[idx].transform);
    }
}
