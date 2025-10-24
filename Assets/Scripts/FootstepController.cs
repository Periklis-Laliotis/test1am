using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    public AudioSource footstepSource;

    [Header("Step Settings")]
    public float stepInterval = 0.5f; // time between steps
    private float stepTimer;

    [Header("Terrain Detection")]
    public LayerMask terrainLayer;
    public LayerMask bridgeLayer;

    private CharacterController cc;
    private Vector3 lastPosition;
    private bool isGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!footstepSource)
            footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        isGrounded = cc.isGrounded;

        if (isGrounded && cc.velocity.magnitude > 0.2f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (CanPlayStepSound())
                {
                    footstepSource.pitch = Random.Range(0.95f, 1.05f);
                    footstepSource.Play();
                }
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    bool CanPlayStepSound()
    {
        // Cast a short ray downward
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 2f))
        {
            GameObject surface = hit.collider.gameObject;

            // --- Terrain check ---
            if (((1 << surface.layer) & terrainLayer) != 0)
            {
                // Player is on terrain
                return false; // skip playing sound
            }

            // --- Bridge check or others ---
            if (((1 << surface.layer) & bridgeLayer) != 0)
            {
                // Example: different sounds for bridge
                // (could swap clip, etc.)
                return true;
            }
        }

        // Default: play
        return true;
    }
}
