using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WindZone))]
public class WindZoneController : MonoBehaviour
{
    private WindZone windZone;

    [Header("Optional Components")]
    [Tooltip("AudioSource that plays wind noise")]
    public AudioSource windAudioSource;

    [Tooltip("Material whose smoothness will change based on transparency value")]
    public Material targetMaterial;

    void Awake()
    {
        windZone = GetComponent<WindZone>();
    }

    void OnEnable()
    {
        ConfigPoller.OnConfigUpdated += HandleConfigUpdate;
    }

    void OnDisable()
    {
        ConfigPoller.OnConfigUpdated -= HandleConfigUpdate;
    }

    private void HandleConfigUpdate(ConfigData data)
    {
        // --- Wind Strength ---
        float clampedSpeed = Mathf.Clamp(data.wind_speed, 1, 5);
        float newMain = 50f + clampedSpeed * 10f; // 1→60, 5→100
        StartCoroutine(SmoothWindChange(newMain));

        // --- Wind Pulse Frequency (Sway) ---
        float clampedSway = Mathf.Clamp(data.sway_effect, 1, 5);
        StartCoroutine(SmoothPulseChange(clampedSway));

        // --- Audio Volume & Pitch ---
        if (windAudioSource)
        {
            float targetVolume = 0.1f + (clampedSpeed - 1) * 0.1f; // 1→0.1, 5→0.5
            StartCoroutine(SmoothAudioChange(targetVolume));
            windAudioSource.pitch = 1f + (clampedSpeed - 1) * 0.05f; // subtle pitch change
        }

        // --- Material Smoothness (Transparency) ---
        if (targetMaterial)
        {
            float clampedTransparency = Mathf.Clamp(data.transparency, 1, 5);
            float targetSmoothness = Mathf.Lerp(0f, 1f, (clampedTransparency - 1f) / 4f);
            StartCoroutine(SmoothSmoothnessChange(targetSmoothness));
        }

        Debug.Log($"🌬️ Config Applied → WindMain: {newMain}, PulseFreq: {clampedSway}, Transparency: {data.transparency}");
    }

    // --- Coroutines ---

    private IEnumerator SmoothWindChange(float target)
    {
        float start = windZone.windMain;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            windZone.windMain = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator SmoothPulseChange(float target)
    {
        float start = windZone.windPulseFrequency;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            windZone.windPulseFrequency = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator SmoothAudioChange(float target)
    {
        float start = windAudioSource.volume;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f; // slower fade
            windAudioSource.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator SmoothSmoothnessChange(float target)
    {
        if (!targetMaterial.HasProperty("_Smoothness"))
            yield break;

        float start = targetMaterial.GetFloat("_Smoothness");
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float newValue = Mathf.Lerp(start, target, t);
            targetMaterial.SetFloat("_Smoothness", newValue);
            yield return null;
        }
    }
}
