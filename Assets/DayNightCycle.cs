using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    public Light targetLight; // The light to rotate
    public float angleMin = 160f; // Minimum x-axis rotation (nighttime)
    public float angleMax = 0f; // Maximum x-axis rotation (daytime)
    public float rotationSpeed = 4f; // Speed of the rotation
    public float buffer = 0.5f; // Buffer to trigger transition
    public float modeSwitchCooldown = 0f; // Cooldown time in seconds before switching modes again

    public Color moonlightColor = new Color(0.6f, 0.7f, 1f); // Moonlight color (customizable)
    public float moonlightIntensity = 0.5f; // Moonlight intensity (customizable)
    public Color sunlightColor = new Color(1f, 0.95f, 0.8f); // Sunlight color
    public float sunlightIntensity = 1f; // Sunlight intensity

    public AudioSource daytimeAudioSource; // Audio source for daytime sounds
    public AudioSource nighttimeAudioSource; // Audio source for nighttime sounds
    public float audioFadeDuration = 2f; // Duration of the audio fade in/out

    public ParticleSystem nightParticles; // The ParticleSystem to fade in/out during transitions
    public float particleFadeDuration = 3f; // Duration of the particle fade

    private float currentRotation; // Tracks the current x-axis rotation
    private bool isNight = false; // Tracks whether it is currently nighttime
    private float lastSwitchTime = 0f; // Time when the mode was last switched

    void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        currentRotation = angleMax; // Start the rotation at angleMax

        // Disable the particle system initially (during the day)
        if (nightParticles != null)
        {
            var main = nightParticles.main;
            main.simulationSpeed = 1f; // Slow down simulation speed during the day
            main.maxParticles = 0; // Set max particles to 0 during the day
        }
    }

    void Update()
    {
        if (targetLight == null) return;

        // Rotate the light continuously
        currentRotation += rotationSpeed * Time.deltaTime;

        // Ensure rotation stays within 0 to 360 degrees
        if (currentRotation >= 360f) currentRotation -= 360f;

        // Only switch modes if enough time has passed since the last switch
        if (Time.time - lastSwitchTime >= modeSwitchCooldown)
        {
            // Check if we are within the buffer range of angleMin (nighttime)
            if (Mathf.Abs(currentRotation - angleMin) < buffer)
            {
                // Only set night mode if it's not already night
                if (!isNight)
                {
                    StartCoroutine(SmoothTransitionToNight());
                    isNight = true; // Update state to night
                    lastSwitchTime = Time.time; // Update the last switch time
                    Debug.Log("It's now nighttime."); // Print message to console
                }
            }
            // Check if we are within the buffer range of angleMax (daytime)
            else if (Mathf.Abs(currentRotation - angleMax) < buffer)
            {
                // Only set day mode if it's not already day
                if (isNight)
                {
                    StartCoroutine(SmoothTransitionToDay());
                    isNight = false; // Update state to day
                    lastSwitchTime = Time.time; // Update the last switch time
                    Debug.Log("It's now daytime."); // Print message to console
                }
            }
        }

        // Apply the rotation directly
        targetLight.transform.rotation = Quaternion.Euler(currentRotation, 0, 0);
    }

    // Smoothly fade an audio source's volume
    IEnumerator FadeAudio(AudioSource audioSource, float targetVolume)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float timeElapsed = 0f;

        while (timeElapsed < audioFadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / audioFadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume; // Ensure the final volume is exact
    }

    // Smooth transition to nighttime mode
    IEnumerator SmoothTransitionToNight()
    {
        float duration = 3f; // Duration of the transition (seconds)
        float startIntensity = targetLight.intensity;
        float targetIntensity = moonlightIntensity;
        Color startColor = targetLight.color;
        Color targetColor = moonlightColor;

        float timeElapsed = 0f;

        // Gradually change both the intensity and the color over time
        while (timeElapsed < duration)
        {
            targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, timeElapsed / duration);
            targetLight.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        targetLight.intensity = targetIntensity; // Ensure the final intensity is exact
        targetLight.color = targetColor; // Ensure the final color is exact
        SetNighttimeMode(); // Set the light to moonlight mode

        // Fade out the particle system
        if (nightParticles != null)
        {
            var main = nightParticles.main;
            main.simulationSpeed = 0.008f; // Slow down the particle system for night
            main.maxParticles = 1000; // Set max particles to 1000

            // Enable the particle system with slow speed
            nightParticles.Play();
        }

        // Fade in nighttime audio and fade out daytime audio
        StartCoroutine(FadeAudio(nighttimeAudioSource, 1f));
        StartCoroutine(FadeAudio(daytimeAudioSource, 0f));
    }

    // Smooth transition to daytime mode
    IEnumerator SmoothTransitionToDay()
    {
        float duration = 3f; // Duration of the transition (seconds)
        float startIntensity = targetLight.intensity;
        float targetIntensity = sunlightIntensity;
        Color startColor = targetLight.color;
        Color targetColor = sunlightColor;

        float timeElapsed = 0f;

        // Gradually change both the intensity and the color over time
        while (timeElapsed < duration)
        {
            targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, timeElapsed / duration);
            targetLight.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        targetLight.intensity = targetIntensity; // Ensure the final intensity is exact
        targetLight.color = targetColor; // Ensure the final color is exact
        SetDaytimeMode(); // Set the light to sunlight mode

        // Fade out the particle system
        if (nightParticles != null)
        {
            var main = nightParticles.main;
            main.simulationSpeed = 1f; // Stop the particle simulation
            main.maxParticles = 0; // Set max particles to 0 to "fade out" the particles

            // Disable particle system simulation
            nightParticles.Stop();
        }

        // Fade out nighttime audio and fade in daytime audio
        StartCoroutine(FadeAudio(nighttimeAudioSource, 0f));
        StartCoroutine(FadeAudio(daytimeAudioSource, 1f));
    }

    // Nighttime mode: Change light settings to imitate moonlight
    void SetNighttimeMode()
    {
        targetLight.color = moonlightColor;
    }

    // Daytime mode: Change light settings to imitate sunlight
    void SetDaytimeMode()
    {
        targetLight.color = sunlightColor;
    }
}
