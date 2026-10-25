using UnityEngine;

/// <summary>
/// 按一次就保持的开关，例如病房呼叫铃。
/// </summary>
public class FlagSwitch : MonoBehaviour, IInteractable
{
    public string flagId = "call301";
    public string label = "呼叫铃";
    public string doneToast = "呼叫铃亮了";

    public string Prompt
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.HasFlag(flagId))
                return string.Empty;
            return "按 E 按下" + label;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance == null || GameManager.Instance.HasFlag(flagId))
            return;
        GameManager.Instance.SetFlag(flagId);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast(doneToast);
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayDoor();
    }
}
