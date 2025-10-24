using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class ConfigData
{
    public int wind_speed;
    public int sway_effect;
    public int transparency;
    
}

public class ConfigPoller : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(PollConfigRoutine());
    }
    public static System.Action<ConfigData> OnConfigUpdated;
    [Header("Server Settings")]
    [Tooltip("The URL to your the JSON output")]
    public string configUrl = "https://xrproject.eu/trust/demo/acrophobia/getConfig.php";

    [Header("Polling Interval (seconds)")]
    public float pollInterval = 3f;

    private ConfigData currentConfig;
    public ConfigData CurrentConfig => currentConfig;

    void Start()
    {
        StartCoroutine(PollConfigRoutine());
    }

    IEnumerator PollConfigRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(GetConfig());
            yield return new WaitForSeconds(pollInterval);
        }
    }

    IEnumerator GetConfig()
    {
        Debug.Log("Attempting to fetch config from: " + configUrl);
        using (UnityWebRequest req = UnityWebRequest.Get(configUrl))
        {
            yield return req.SendWebRequest();
            Debug.Log($"Request finished with result: {req.result}, code: {req.responseCode}, error: {req.error}");
            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                Debug.Log("Received config: " + json);

                try
                {
                    ConfigData newConfig = JsonUtility.FromJson<ConfigData>(json);
                    ApplyConfig(newConfig);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error fetching config: " + req.error);
            }
        }
    }
    void ApplyConfig(ConfigData config)
    {
        currentConfig = config;
        Debug.Log($"Applied Config -> Wind Speed: {config.wind_speed}, Sway Effect: {config.sway_effect}, Transparency: {config.transparency}");
        OnConfigUpdated?.Invoke(config);
        // Example of dynamic application:
        // Adjust environment or object parameters here:
        // e.g., windZone.windMain = config.wind_speed;
        //       treeAnimator.SetFloat("SwayIntensity", config.sway_effect);
        //       material.SetFloat("_Transparency", config.transparency / 5f);
    }
}
