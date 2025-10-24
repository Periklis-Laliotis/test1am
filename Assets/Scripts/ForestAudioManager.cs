using System.Collections;
using UnityEngine;

public class ForestAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource windSource;
    public AudioSource birdsSource;

    [Header("Audio Clips")]
    public AudioClip[] birdClips;
    public AudioClip windLoop;

    [Header("Settings")]
    public Vector2 birdDelayRange = new Vector2(5f, 15f);
    public Vector2 birdVolumeRange = new Vector2(0.4f, 0.8f);
    public float windVolume = 0.3f;

    private Coroutine birdRoutine;

    void Start()
    {
        // Start looping wind
        if (windSource && windLoop)
        {
            windSource.clip = windLoop;
            windSource.loop = true;
            windSource.volume = windVolume;
            windSource.Play();
        }

        // Start bird routine
        if (birdsSource && birdClips.Length > 0)
        {
            birdRoutine = StartCoroutine(PlayBirdSounds());
        }
    }

    IEnumerator PlayBirdSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(birdDelayRange.x, birdDelayRange.y));

            AudioClip clip = birdClips[Random.Range(0, birdClips.Length)];
            birdsSource.pitch = Random.Range(0.9f, 1.1f);
            birdsSource.volume = Random.Range(birdVolumeRange.x, birdVolumeRange.y);

            birdsSource.clip = clip;
            birdsSource.loop = false;
            birdsSource.Play(); // ✅ παίζει μόνο ένα clip κάθε φορά
        }
    }

    // ✅ Stop ή fade-out των πουλιών όταν φύγουμε από το δάσος
    public void StopBirds(float fadeDuration = 1.5f)
    {
        if (birdRoutine != null)
        {
            StopCoroutine(birdRoutine);
            birdRoutine = null;
        }

        StartCoroutine(FadeOutBirds(fadeDuration));
    }

    private IEnumerator FadeOutBirds(float duration)
    {
        float startVol = birdsSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            birdsSource.volume = Mathf.Lerp(startVol, 0, time / duration);
            yield return null;
        }

        birdsSource.Stop();
        birdsSource.volume = startVol; // προετοιμασία για επόμενη φορά (αν χρειαστεί)
    }

    
    // ✅ Ξαναρχίζει το ambiance του δάσους (πουλιά + άνεμος)
public void RestartBirds()
{
    if (birdsSource && birdClips.Length > 0)
    {
        if (birdRoutine == null)
            birdRoutine = StartCoroutine(PlayBirdSounds());
    }

    if (windSource && !windSource.isPlaying && windLoop != null)
    {
        windSource.clip = windLoop;
        windSource.loop = true;
        windSource.volume = windVolume;
        windSource.Play();
    }

    Debug.Log("🌲 Forest ambiance restarted!");
}

}
