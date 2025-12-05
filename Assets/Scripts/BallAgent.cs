using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallAgent : MonoBehaviour
{
    Rigidbody rBody;
    public EnvironmentController env;

    [Header("Movement")]
    public float forceMultiplier = 12f;
    public float maxSpeed = 6f;
    public float steerResponsiveness = 6f;
    public float reachStopDistance = 0.6f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 1.2f;
    public float avoidForce = 8f;
    public float raycastSpacing = 0.4f;
    public LayerMask obstacleMask;

    [Header("Wander")]
    public float wanderStrength = 0.6f;
    public float wanderChangeFreq = 1.0f;
    private Vector3 wanderVec = Vector3.zero;
    private float wanderTimer = 0f;

    [Header("Debug")]
    public bool drawDebugRays = true;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        if (env == null) Debug.LogWarning("ScriptedAgent: EnvironmentController not assigned.");
    }

    void FixedUpdate()
    {
        if (env == null) return;

        Transform target = env.GetNearestPickup(transform.position);

        Vector3 desiredDir = Vector3.zero;
        float targetDist = float.MaxValue;
        if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            targetDist = toTarget.magnitude;
            desiredDir = toTarget.normalized;
        }
        else
        {
            wanderTimer += Time.fixedDeltaTime;
            if (wanderTimer >= wanderChangeFreq)
            {
                wanderTimer = 0f;
                wanderVec = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            }
            desiredDir = wanderVec * wanderStrength;
        }

        Vector3 avoidVec = Vector3.zero;
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        RaycastHit hit;
        if (Physics.Raycast(origin, forward, out hit, avoidDistance, obstacleMask))
        {
            avoidVec += hit.normal * (avoidDistance - hit.distance);
            if (drawDebugRays) Debug.DrawRay(origin, forward * hit.distance, Color.red, 0.1f);
        }
        else if (drawDebugRays) Debug.DrawRay(origin, forward * avoidDistance, Color.green, 0.1f);

        Vector3 leftDir = Quaternion.Euler(0f, -30f, 0f) * forward;
        if (Physics.Raycast(origin, leftDir, out hit, avoidDistance, obstacleMask))
        {
            avoidVec += hit.normal * (avoidDistance - hit.distance);
            if (drawDebugRays) Debug.DrawRay(origin, leftDir * hit.distance, Color.red, 0.1f);
        }
        else if (drawDebugRays) Debug.DrawRay(origin, leftDir * avoidDistance, Color.green, 0.1f);

        Vector3 rightDir = Quaternion.Euler(0f, 30f, 0f) * forward;
        if (Physics.Raycast(origin, rightDir, out hit, avoidDistance, obstacleMask))
        {
            avoidVec += hit.normal * (avoidDistance - hit.distance);
            if (drawDebugRays) Debug.DrawRay(origin, rightDir * hit.distance, Color.red, 0.1f);
        }
        else if (drawDebugRays) Debug.DrawRay(origin, rightDir * avoidDistance, Color.green, 0.1f);

        Vector3 combined = desiredDir + avoidVec * avoidForce;
        if (combined.sqrMagnitude < 1e-6f) combined = transform.forward;

        combined.y = 0f;
        combined.Normalize();

        Vector3 currentVel = rBody.linearVelocity;
        Vector3 desiredVel = combined * maxSpeed;
        Vector3 steer = (desiredVel - currentVel) * steerResponsiveness;

        if (target != null && targetDist <= reachStopDistance)
        {
            steer *= 0.2f;
        }

        rBody.AddForce(steer * forceMultiplier, ForceMode.Force);

        Vector3 horVel = new Vector3(rBody.linearVelocity.x, 0f, rBody.linearVelocity.z);
        if (horVel.magnitude > maxSpeed)
        {
            Vector3 clamped = horVel.normalized * maxSpeed;
            rBody.linearVelocity = new Vector3(clamped.x, rBody.linearVelocity.y, clamped.z);
        }
    }
}
