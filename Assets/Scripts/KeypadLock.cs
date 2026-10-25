using UnityEngine;

/// <summary>
/// 门禁键盘。对准后按 E，用数字键输入，回车确认。
/// </summary>
public class KeypadLock : MonoBehaviour, IInteractable
{
    public string code = "0000";
    public string title = "门禁";
    public DoorController door;
    public string flagOnSuccess;

    public string Prompt
    {
        get
        {
            if (door != null && door.IsOpen)
                return string.Empty;
            return "按 E 输入" + title;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (door != null && door.IsOpen)
            return;
        if (GameManager.Instance != null)
            GameManager.Instance.BeginCode(this);
    }

    public void Succeed()
    {
        if (door != null)
            door.ForceOpen();
        if (GameManager.Instance != null && !string.IsNullOrEmpty(flagOnSuccess))
            GameManager.Instance.SetFlag(flagOnSuccess);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast(title + "开了");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayDoor();
    }
}
