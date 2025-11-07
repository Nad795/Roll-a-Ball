using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class BallAgent : Agent
{
    [Header("Agent Components")]
    public Rigidbody rb;
    public Transform enemy; // musuh (NavMeshAgent)
    
    private Transform currentTarget;
    private Vector3 lastTargetPosition;

    [Header("Agent Settings")]
    public float moveForce = 10f;
    public float stepPenalty = -0.001f;
    public float proximityPenalty = -0.01f; // penalti dekat musuh
    public float distanceRewardFactor = 0.1f; // reward progresif jarak ke target

    public override void OnEpisodeBegin()
    {
        // Reset posisi & velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0.5f, 0);

        // Aktifkan semua pickup
        foreach (var pickup in GameObject.FindGameObjectsWithTag("PickUp"))
            pickup.SetActive(true);

        // Set target awal
        UpdateTarget();
        if (currentTarget != null)
            lastTargetPosition = currentTarget.localPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (currentTarget != null)
        {
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(currentTarget.localPosition);
            sensor.AddObservation(rb.linearVelocity.x);
            sensor.AddObservation(rb.linearVelocity.z);

            if (enemy != null)
                sensor.AddObservation(enemy.localPosition);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 force = new Vector3(moveX, 0, moveZ);
        rb.AddForce(force * moveForce);

        // Reward progresif: semakin dekat ke target
        if (currentTarget != null)
        {
            float oldDistance = Vector3.Distance(transform.localPosition, lastTargetPosition);
            float newDistance = Vector3.Distance(transform.localPosition, currentTarget.localPosition);
            float delta = oldDistance - newDistance;
            AddReward(delta * distanceRewardFactor);
            lastTargetPosition = currentTarget.localPosition;
        }

        // Penalti kecil tiap step supaya agent tidak diam
        AddReward(stepPenalty);

        // Penalti dekat musuh
        if (enemy != null)
        {
            float dist = Vector3.Distance(transform.localPosition, enemy.localPosition);
            if (dist < 1.0f) AddReward(proximityPenalty);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            AddReward(1.0f); // reward pickup
            UpdateTarget();

            // Selesai jika semua pickup sudah diambil
            if (AllPickUpsCollected())
                EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1.0f); // penalti tertangkap musuh
            EndEpisode();
        }
    }

    private bool AllPickUpsCollected()
    {
        foreach (var pickup in GameObject.FindGameObjectsWithTag("PickUp"))
            if (pickup.activeSelf) return false;
        return true;
    }

    private void UpdateTarget()
    {
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var pickup in GameObject.FindGameObjectsWithTag("PickUp"))
        {
            if (!pickup.activeSelf) continue;
            float dist = Vector3.Distance(transform.localPosition, pickup.transform.localPosition);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = pickup.transform;
            }
        }
        currentTarget = nearest;
    }
}
