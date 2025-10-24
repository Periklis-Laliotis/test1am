using UnityEngine;

public class BridgeBuilder : MonoBehaviour
{
    [Header("Bridge Setup")]
    public Transform startAnchor;
    public Transform endAnchor;
    public GameObject plankPrefab;
    [Range(2, 100)] public int plankCount = 15;
    public float plankSpacing = 0.6f;
    public float sideOffset = 0.3f;  // left/right rope spacing
    public float anchorOffset = -0.2f; // forward offset along plank

    [Header("Physics")]
    public float plankMass = 5f;
    public bool autoConnectEnds = true;

    void Start()
    {
        if (!plankPrefab || !startAnchor || !endAnchor)
        {
            Debug.LogError("BridgeBuilder: Missing references!");
            return;
        }

        BuildBridge();
    }

    void BuildBridge()
    {
        Vector3 dir = (endAnchor.position - startAnchor.position).normalized;
        float totalLength = Vector3.Distance(startAnchor.position, endAnchor.position);
        float spacing = totalLength / plankCount;

        Rigidbody prevBody = null;

        for (int i = 0; i < plankCount; i++)
        {
            Vector3 pos = startAnchor.position + dir * spacing * i;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            GameObject plank = Instantiate(plankPrefab, pos, rot, transform);

            // --- Get the correct Rigidbody ---
            Rigidbody rb = GetNonKinematicRigidbody(plank);
            if (!rb)
            {
                Debug.LogWarning($"BridgeBuilder: No non-kinematic Rigidbody found on plank {i}, skipping.");
                continue;
            }

            rb.mass = plankMass;
            rb.drag = 1f;
            rb.angularDrag = 0.4f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (prevBody != null)
            {
                // LEFT hinge
                var hjLeft = plank.AddComponent<HingeJoint>();
                hjLeft.connectedBody = prevBody;
                hjLeft.anchor = new Vector3(-sideOffset, 0f, anchorOffset);
                hjLeft.axis = Vector3.right;
                hjLeft.useLimits = true;
                hjLeft.limits = new JointLimits { min = -15f, max = 15f };

                // RIGHT hinge
                var hjRight = plank.AddComponent<HingeJoint>();
                hjRight.connectedBody = prevBody;
                hjRight.anchor = new Vector3(sideOffset, 0f, anchorOffset);
                hjRight.axis = Vector3.right;
                hjRight.useLimits = true;
                hjRight.limits = new JointLimits { min = -15f, max = 15f };
            }

            prevBody = rb;
        }

        // Connect bridge ends
        if (autoConnectEnds)
        {
            Rigidbody first = GetNonKinematicRigidbody(transform.GetChild(0).gameObject);
            Rigidbody last = GetNonKinematicRigidbody(transform.GetChild(transform.childCount - 1).gameObject);

            if (startAnchor)
            {
                var hj1 = first.gameObject.AddComponent<HingeJoint>();
                hj1.connectedBody = RequireKinematic(startAnchor);
                hj1.anchor = new Vector3(0, 0, anchorOffset);
                hj1.axis = Vector3.right;
            }

            if (endAnchor)
            {
                var hj2 = last.gameObject.AddComponent<HingeJoint>();
                hj2.connectedBody = RequireKinematic(endAnchor);
                hj2.anchor = new Vector3(0, 0, -anchorOffset);
                hj2.axis = Vector3.right;
            }
        }

        Debug.Log($"Bridge built with {plankCount} planks using dual hinges (non-kinematic only).");
    }

    // --- Utility: only return real dynamic RB ---
    Rigidbody GetNonKinematicRigidbody(GameObject obj)
    {
        Rigidbody[] allBodies = obj.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allBodies)
        {
            if (!rb.isKinematic)
                return rb; // Found our physics body
        }
        return null;
    }

    // --- Ensure anchors are kinematic ---
    Rigidbody RequireKinematic(Transform t)
    {
        var rb = t.GetComponent<Rigidbody>();
        if (!rb) rb = t.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        return rb;
    }
}
