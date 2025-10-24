using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StickyPlatformCC : MonoBehaviour
{
    private Transform platform;
    private CharacterController targetCC;
    private Vector3 lastPlatformPos;
    private Quaternion lastPlatformRot;
    private bool following;

    [Header("Advanced Settings")]
    [Tooltip("Extra offset smoothing factor for rotations (0 = instant, 1 = very smooth)")]
    [Range(0f, 1f)] public float rotationLerp = 0.15f;

    [Tooltip("Apply correction if platform moves down quickly")]
    public bool correctDownwardMotion = true;

    private Vector3 pendingMove;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        platform = transform.parent != null ? transform.parent : transform;
        lastPlatformPos = platform.position;
        lastPlatformRot = platform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        var cc = other.GetComponentInParent<CharacterController>();
        if (cc != null)
        {
            targetCC = cc;
            following = true;
            pendingMove = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == targetCC)
        {
            following = false;
            targetCC = null;
        }
    }

    void FixedUpdate()
    {
        if (!following || targetCC == null)
        {
            lastPlatformPos = platform.position;
            lastPlatformRot = platform.rotation;
            return;
        }

        // Compute delta from last physics step
        Vector3 deltaPos = platform.position - lastPlatformPos;
        Quaternion deltaRot = platform.rotation * Quaternion.Inverse(lastPlatformRot);

        // Compute rotation offset for player relative to platform center
        Vector3 relativePos = targetCC.transform.position - platform.position;
        Vector3 rotated = deltaRot * relativePos - relativePos;

        // Total delta to apply
        pendingMove = deltaPos + rotated * (1f - rotationLerp);

        // Small correction if platform moves downward fast
        if (correctDownwardMotion && deltaPos.y < -0.01f)
            pendingMove.y += deltaPos.y;

        if (pendingMove.sqrMagnitude > 0.000001f)
            targetCC.Move(pendingMove);

        // Store for next frame
        lastPlatformPos = platform.position;
        lastPlatformRot = platform.rotation;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (!col) return;

        Gizmos.color = following ? new Color(0.2f, 1f, 0.2f, 0.4f) : new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
#endif
}
