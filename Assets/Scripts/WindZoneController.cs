using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WindZone))]
public class WindZoneController : MonoBehaviour
{
    private WindZone windZone;
    [Tooltip("Optional: link to an AudioSource that plays wind noise")]
    public AudioSource windAudioSource;

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

        // --- Wind Pulse Frequency (sway effect) ---
        float clampedSway = Mathf.Clamp(data.sway_effect, 1, 5);
        StartCoroutine(SmoothPulseChange(clampedSway));

        // --- Audio Volume ---
        if (windAudioSource)
        {
            float targetVolume = 0.1f + (clampedSpeed - 1) * 0.1f; // 1→0.1, 5→0.5
            StartCoroutine(SmoothAudioChange(targetVolume));

            // Optional: pitch variation adds realism
            windAudioSource.pitch = 1f + (clampedSpeed - 1) * 0.05f; // 1→1.0, 5→1.2
        }

        Debug.Log($"🌬️ WindZone updated → Main: {newMain}, PulseFreq: {clampedSway}, AudioVol: {(windAudioSource ? windAudioSource.volume.ToString("F2") : "none")}");
    }

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
}
