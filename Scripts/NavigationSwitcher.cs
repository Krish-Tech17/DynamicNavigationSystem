using UnityEngine;

public class NavigationSwitcher : MonoBehaviour
{
    public bool isInSecondary = false;

    [Header("Primary Systems")]
    public GuidedPathRenderer primaryRenderer;
    public NavigationMonitor primaryMonitor;
    public NavigationArrowController primaryArrow;

    [Header("Secondary Systems")]
    public SecondaryGuidedPathRenderer secondaryRenderer;
    public SecondaryNavigationMonitor secondaryMonitor;
    public SecondaryNavigationArrowController secondaryArrow;

    void Start()
    {
        EnablePrimary();
    }

    public void EnterSecondary()
    {
        isInSecondary = true;
        primaryRenderer.gameObject.SetActive(false);
        primaryArrow.gameObject.SetActive(false);
        secondaryRenderer.gameObject.SetActive(true);
    }

    public void ExitSecondary()
    {
        isInSecondary = false;
        secondaryRenderer.gameObject.SetActive(false);
        secondaryArrow.gameObject.SetActive(false);
        primaryRenderer.gameObject.SetActive(true);
    }

    void EnablePrimary()
    {
        primaryRenderer.gameObject.SetActive(true);
        secondaryRenderer.gameObject.SetActive(false);
    }
}
