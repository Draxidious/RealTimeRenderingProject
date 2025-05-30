using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WindEffectController : MonoBehaviour
{
    [Tooltip("Wind Zone whose properties will be modified.")]
    public WindZone windZone;

    [Tooltip("Fire particle systems whose rotation will be modified.")]
    public List<ParticleSystem> fireParticleSystems;

    [Tooltip("Increase in turbulence for the wind effect.")]
    public float turbulenceIncrease = 1.0f;

    [Tooltip("Increase in main wind force.")]
    public float mainWindIncrease = 2.0f;

    [Tooltip("Increase in pulse magnitude of wind.")]
    public float pulseMagnitudeIncrease = 1.0f;

    [Tooltip("Increase in pulse frequency of wind.")]
    public float pulseFrequencyIncrease = 0.5f;

    [Tooltip("The duration the particles will emit.")]
    public float emissionDuration = 3.0f;

    [Tooltip("Minimum and maximum delay before particles start emitting again.")]
    public Vector2 randomDelayRange = new Vector2(2.0f, 5.0f);

    [Tooltip("Duration for audio fade in and fade out, and wind zone transition.")]
    public float fadeDuration = 1.0f;

    [Tooltip("Multiplier to control how much the fire rotates based on the wind.")]
    public float fireRotationMultiplier = 1.0f;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private AudioSource audioSource;

    // Store initial wind settings
    private float initialTurbulence;
    private float initialMainWind;
    private float initialPulseMagnitude;
    private float initialPulseFrequency;
    private float initialVolume;

    private void Start()
    {
        // Get all particle systems on the GameObject
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());

        // Store initial wind properties
        if (windZone != null)
        {
            initialTurbulence = windZone.windTurbulence;
            initialMainWind = windZone.windMain;
            initialPulseMagnitude = windZone.windPulseMagnitude;
            initialPulseFrequency = windZone.windPulseFrequency;
        }

        // Set up the audio source
        audioSource = GetComponent<AudioSource>();
        initialVolume = audioSource.volume;

        // Start the coroutine to control the wind effect
        StartCoroutine(WindEffectRoutine());
    }

    private IEnumerator WindEffectRoutine()
    {
        while (true)
        {
            // Wait for a random delay
            float delay = Random.Range(randomDelayRange.x, randomDelayRange.y);
            yield return new WaitForSeconds(delay);

            // Start particle emission
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = true;
            }

            // Adjust fire particle system rotation based on wind direction
            StartCoroutine(FadeFireParticleRotation(true));

            // Play wind sound with random start time, ensuring it plays for the full duration
            if (audioSource != null && audioSource.clip != null)
            {
                float clipLength = audioSource.clip.length;
                float maxStartTime = Mathf.Max(0, clipLength - emissionDuration); // Ensure enough time for full play
                audioSource.time = Random.Range(0, maxStartTime);
                audioSource.volume = 0; // Start muted for fade-in
                audioSource.Play();

                // Start the fade-in
                StartCoroutine(FadeInAudio());
            }

            // Fade in wind zone properties
            StartCoroutine(FadeInWindZoneProperties());

            // Wait for the emission duration minus fade-out time
            yield return new WaitForSeconds(emissionDuration - fadeDuration);

            // Start fading out the audio
            StartCoroutine(FadeOutAudio());

            // Fade out wind zone properties
            StartCoroutine(FadeOutWindZoneProperties());

            // Wait for fade-out to complete
            yield return new WaitForSeconds(fadeDuration);

            // Stop particle emission
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.enabled = false;
            }

            // Fade fire particle rotation back to normal (opposite direction)
            StartCoroutine(FadeFireParticleRotation(false));
        }
    }

    private IEnumerator FadeFireParticleRotation(bool fadeIn)
    {
        // Determine wind direction based on wind zone's forward vector
        Vector3 targetWindDirection = windZone.transform.forward;
        Vector3 initialWindDirection = fireParticleSystems[0].shape.rotation; // Assuming all fire particles have similar initial direction

        if (!fadeIn)
        {
            // Revert back to original rotation when fading out
            targetWindDirection = Vector3.zero; // Adjust this as needed to return to original
        }

        float lerpTime = fadeIn ? fadeDuration : fadeDuration / 2; // You can adjust fade out duration

        for (float t = 0; t < lerpTime; t += Time.deltaTime)
        {
            float lerpFactor = t / lerpTime;
            // Apply smooth rotation change to each fire particle system
            foreach (var firePS in fireParticleSystems)
            {
                var shapeModule = firePS.shape;

                // Gradually change the rotation to match the wind direction
                shapeModule.rotation = Vector3.Lerp(initialWindDirection, targetWindDirection * fireRotationMultiplier, lerpFactor);
            }

            yield return null;
        }

        // Ensure final rotation is set
        foreach (var firePS in fireParticleSystems)
        {
            var shapeModule = firePS.shape;
            shapeModule.rotation = targetWindDirection * fireRotationMultiplier;
        }
    }

    private IEnumerator FadeInAudio()
    {
        float startVolume = 0;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, initialVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = initialVolume;
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    private IEnumerator FadeInWindZoneProperties()
    {
        float startTurbulence = windZone.windTurbulence;
        float startMainWind = windZone.windMain;
        float startPulseMagnitude = windZone.windPulseMagnitude;
        float startPulseFrequency = windZone.windPulseFrequency;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float lerpFactor = t / fadeDuration;
            windZone.windTurbulence = Mathf.Lerp(startTurbulence, initialTurbulence + turbulenceIncrease, lerpFactor);
            windZone.windMain = Mathf.Lerp(startMainWind, initialMainWind + mainWindIncrease, lerpFactor);
            windZone.windPulseMagnitude = Mathf.Lerp(startPulseMagnitude, initialPulseMagnitude + pulseMagnitudeIncrease, lerpFactor);
            windZone.windPulseFrequency = Mathf.Lerp(startPulseFrequency, initialPulseFrequency + pulseFrequencyIncrease, lerpFactor);
            yield return null;
        }

        windZone.windTurbulence = initialTurbulence + turbulenceIncrease;
        windZone.windMain = initialMainWind + mainWindIncrease;
        windZone.windPulseMagnitude = initialPulseMagnitude + pulseMagnitudeIncrease;
        windZone.windPulseFrequency = initialPulseFrequency + pulseFrequencyIncrease;
    }

    private IEnumerator FadeOutWindZoneProperties()
    {
        float startTurbulence = windZone.windTurbulence;
        float startMainWind = windZone.windMain;
        float startPulseMagnitude = windZone.windPulseMagnitude;
        float startPulseFrequency = windZone.windPulseFrequency;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float lerpFactor = t / fadeDuration;
            windZone.windTurbulence = Mathf.Lerp(startTurbulence, initialTurbulence, lerpFactor);
            windZone.windMain = Mathf.Lerp(startMainWind, initialMainWind, lerpFactor);
            windZone.windPulseMagnitude = Mathf.Lerp(startPulseMagnitude, initialPulseMagnitude, lerpFactor);
            windZone.windPulseFrequency = Mathf.Lerp(startPulseFrequency, initialPulseFrequency, lerpFactor);
            yield return null;
        }

        windZone.windTurbulence = initialTurbulence;
        windZone.windMain = initialMainWind;
        windZone.windPulseMagnitude = initialPulseMagnitude;
        windZone.windPulseFrequency = initialPulseFrequency;
    }
}
