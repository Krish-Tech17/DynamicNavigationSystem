using UnityEngine;

public class SecondaryTrigger : MonoBehaviour
{
    public NavigationSwitcher switcher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            switcher.EnterSecondary();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            switcher.ExitSecondary();
    }
}
