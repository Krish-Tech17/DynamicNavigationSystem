using UnityEngine;

public class SecondaryTrigger : MonoBehaviour
{
    public NavigationSwitcher switcher;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switcher.EnterSecondary();
            Debug.Log("On Secondary"+ gameObject.name);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Player"))
        //    switcher.ExitSecondary();
    }
}
