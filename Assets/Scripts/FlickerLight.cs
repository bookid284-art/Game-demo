using UnityEngine;

/// <summary>
/// 日光灯不规则闪烁。
/// </summary>
[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    public float baseIntensity = 1.2f;
    public float speed = 7f;

    Light lamp;
    float seed;

    void Awake()
    {
        lamp = GetComponent<Light>();
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (lamp == null)
            return;
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        if (n > 0.84f)
            lamp.intensity = baseIntensity * Random.Range(0.02f, 0.25f);
        else if (n > 0.72f)
            lamp.intensity = baseIntensity * Random.Range(0.35f, 0.7f);
        else
            lamp.intensity = baseIntensity * Mathf.Lerp(0.82f, 1f, n);
    }
}
