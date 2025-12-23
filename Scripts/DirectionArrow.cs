using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform target;

    [Header("Arrow Settings")]
    public float heightOffset = 1.5f;
    public float rotateSpeed = 5f;

    void Update()
    {
        if (player == null || target == null) return;

        // Position arrow above player
        Vector3 pos = player.position;
        pos.y += heightOffset;
        transform.position = pos;

        // Rotation logic (Y-axis only)
        Vector3 dir = target.position - transform.position;
        dir.y = 0f; // ⭐ Remove vertical component to stay flat

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir.normalized);

        // ⭐ Lock rotation to Y-axis only
        Vector3 e = lookRot.eulerAngles;
        e.x = 0f;
        e.z = 0f;
        lookRot = Quaternion.Euler(e);

        // Smooth rotate
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
