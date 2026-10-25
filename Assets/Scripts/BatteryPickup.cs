using UnityEngine;

/// <summary>
/// 手电筒电池。
/// </summary>
public class BatteryPickup : MonoBehaviour, IInteractable
{
    public float amount = 0.45f;

    public string Prompt
    {
        get { return "按 E 拾取电池"; }
    }

    public void Interact(PlayerInteractor interactor)
    {
        FlashlightController light = interactor != null
            ? interactor.GetComponent<FlashlightController>()
            : null;
        if (light != null)
            light.AddBattery(amount);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("电池补充了");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
