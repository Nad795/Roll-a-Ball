using UnityEngine;

public class Pickup : MonoBehaviour
{
    public EnvironmentController env;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            env.NotifyPickupTaken(gameObject);
    }
}
