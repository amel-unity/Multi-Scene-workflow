using UnityEngine;

public class TriggerDetection : MonoBehaviour
{
    private bool state;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Controller>())
        {
            state = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Controller>())
        {
            state = false;
        }
    }

    public bool GetState()
    {
        return state;
    }
}
