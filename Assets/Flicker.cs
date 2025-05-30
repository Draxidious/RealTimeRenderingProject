using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker : MonoBehaviour
{
    // Reference to the point light component
    private Light pointLight;

    // Parameters to control the intensity flickering
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 0.1f;

    // Time accumulator to smooth the flicker effect
    private float flickerTime = 0f;

    void Start()
    {
        // Get the Light component attached to this GameObject
        pointLight = GetComponent<Light>();
    }

    void Update()
    {
        // Randomly flicker the light's intensity between minIntensity and maxIntensity
        flickerTime += Time.deltaTime * flickerSpeed;
        pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(flickerTime, 0f));
    }
}
