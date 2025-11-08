using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveDirection = Vector3.right; // arah gerak (kanan default)
    public float distance = 3f;                   // jarak gerak bolak-balik
    public float speed = 2f;                      // kecepatan gerak

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Gerak bolak-balik dengan Mathf.PingPong
        transform.position = startPos + moveDirection.normalized *
            (Mathf.PingPong(Time.time * speed, distance) - distance / 2);
    }
}
