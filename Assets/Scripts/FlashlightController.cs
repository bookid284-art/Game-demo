using UnityEngine;

/// <summary>
/// 手电筒。F 或鼠标右键开关，电量耗尽后需要拾取电池。
/// </summary>
public class FlashlightController : MonoBehaviour
{
    public Light spot;
    public float battery = 1f;
    public float drainPerSecond = 0.0115f;
    public float drainMultiplier = 1f;
    public bool isOn = true;

    float flickerClock;

    public float Battery01
    {
        get { return Mathf.Clamp01(battery); }
    }

    public void AddBattery(float amount)
    {
        battery = Mathf.Clamp01(battery + amount);
        if (battery > 0.02f)
            isOn = true;
        Apply();
    }

    void Update()
    {
        bool blocked = GameManager.Instance != null && GameManager.Instance.BlocksPlayer;
        if (!blocked && (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(1)))
            Toggle();

        if (isOn)
        {
            battery -= drainPerSecond * drainMultiplier * Time.deltaTime;
            if (battery <= 0f)
            {
                battery = 0f;
                isOn = false;
                if (GameHUD.Instance != null)
                    GameHUD.Instance.Toast("电池耗尽");
            }
        }

        Apply();
    }

    void Toggle()
    {
        if (!isOn && battery <= 0.01f)
        {
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast("没有电了");
            return;
        }
        isOn = !isOn;
    }

    void Apply()
    {
        if (spot == null)
            return;

        float intensity = 2.6f;
        if (isOn && battery < 0.18f)
        {
            flickerClock += Time.deltaTime * 28f;
            float n = Mathf.PerlinNoise(flickerClock, 0.3f);
            if (n > 0.72f)
                intensity *= Random.Range(0.15f, 0.45f);
        }

        spot.enabled = isOn && battery > 0.001f;
        spot.intensity = intensity;
    }
}
