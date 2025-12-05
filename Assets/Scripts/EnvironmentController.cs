using UnityEngine;
using System.Collections.Generic;

public class EnvironmentController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject pickupPrefab;

    [Header("Environment Settings")]
    public int pickupCount = 10;
    public Vector2 spawnRange = new Vector2(8f, 8f);

    private List<GameObject> pickups = new List<GameObject>();

    void Start()
    {
        ResetEnvironment();
    }

    public void ResetEnvironment()
    {
        foreach (var p in pickups)
            if (p != null) Destroy(p);
        pickups.Clear();

        for (int i = 0; i < pickupCount; i++)
        {
            SpawnPickup();
        }

        if (player != null)
        {
            player.position = new Vector3(0f, 0.5f, 0f);
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    private void SpawnPickup()
    {
        Vector3 pos = new Vector3(
            Random.Range(-spawnRange.x, spawnRange.x),
            0.4f,
            Random.Range(-spawnRange.y, spawnRange.y)
        );

        GameObject newPickup = Instantiate(pickupPrefab, pos, Quaternion.identity, transform);
        pickups.Add(newPickup);

        Pickup pickupScript = newPickup.GetComponent<Pickup>();
        if (pickupScript != null)
            pickupScript.env = this;
    }

    public void NotifyPickupTaken(GameObject pickup)
    {
        pickups.Remove(pickup);
        Destroy(pickup);

        SpawnPickup();
    }

    public Transform GetNearestPickup(Vector3 from)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var p in pickups)
        {
            if (p == null) continue;
            float d = Vector3.Distance(from, p.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = p.transform;
            }
        }
        return nearest;
    }
}
