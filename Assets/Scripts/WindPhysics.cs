using UnityEngine;
using System.Collections;
/// <summary>
/// Applies physical wind force from a WindZone to all rigidbodies under this object.
/// </summary>
public class WindPhysics : MonoBehaviour
{
    [Header("Wind Setup")]
    public WindZone windZone;
    [Tooltip("Multiplier for wind strength applied to rigidbodies.")]
    public float forceMultiplier = 1.0f;

    [Header("Filtering")]
    [Tooltip("Only apply force to these rigidbodies (optional). If empty, applies to all children.")]
    public Rigidbody[] targetBodies;

    private Vector3 baseWindDir;

    IEnumerator Start()
    {
        if (!windZone)
            windZone = FindObjectOfType<WindZone>();

        if (!windZone)
        {
            Debug.LogWarning("WindPhysics: No WindZone found!");
            enabled = false;
            yield break;
        }

        // Wait a short moment to let the bridge finish building
        yield return new WaitForSeconds(0.2f);

        // Now collect rigidbodies
        targetBodies = GetComponentsInChildren<Rigidbody>();
        Debug.Log($"WindPhysics: Found {targetBodies.Length} rigidbodies after build under {gameObject.name}");
    }

    void FixedUpdate()
    {
        ApplyWind();
    }

    void ApplyWind()
    {
        if (windZone.mode == WindZoneMode.Directional)
        {
            // Directional wind: use WindZone forward direction
            Vector3 windDir = windZone.transform.forward.normalized;

            // Base intensity
            float strength = windZone.windMain * forceMultiplier;

            // Add turbulence
            strength += Mathf.PerlinNoise(Time.time * windZone.windPulseFrequency, 0f) * windZone.windTurbulence;

            // Random pulsing (simulates gusts)
            float pulse = 1f + Mathf.Sin(Time.time * windZone.windPulseFrequency) * windZone.windPulseMagnitude;

            // Final wind vector
            Vector3 windForce = windDir * strength * pulse;
            Debug.DrawRay(windZone.transform.position, windForce.normalized * 5f, Color.cyan);

            // Apply to all target rigidbodies
            foreach (var rb in targetBodies)
            {
                if (rb && !rb.isKinematic)
                {
                    rb.AddForce(windForce, ForceMode.Force);
                    Debug.DrawRay(rb.worldCenterOfMass, windForce.normalized * 2f, Color.yellow);
                }
            }
        }
        else if (windZone.mode == WindZoneMode.Spherical)
        {
            // Spherical wind: apply force radially from the WindZone position
            foreach (var rb in targetBodies)
            {
                if (!rb || rb.isKinematic) continue;

                Vector3 toRB = rb.worldCenterOfMass - windZone.transform.position;
                float distance = toRB.magnitude;

                // Falloff by distance
                float attenuation = Mathf.Clamp01(1f - distance / windZone.radius);

                Vector3 windForce = toRB.normalized * windZone.windMain * attenuation * forceMultiplier;
                rb.AddForce(windForce, ForceMode.Force);
            }
        }
    }
}
