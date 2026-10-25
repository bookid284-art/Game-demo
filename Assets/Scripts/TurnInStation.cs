using UnityEngine;

/// <summary>
/// 把支线物品交回指定地点。
/// </summary>
public class TurnInStation : MonoBehaviour, IInteractable
{
    public string title = "归还";
    public string requires = "";
    public string doneFlag = "pharmacy_done";
    public string reward = "drain";
    public string lockedHint = "东西还没齐";
    public string doneToast = "归还了";

    public string Prompt
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.HasFlag(doneFlag))
                return string.Empty;
            return "按 E " + title;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance == null || GameManager.Instance.HasFlag(doneFlag))
            return;
        if (!Ready())
        {
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast(lockedHint);
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayLocked();
            return;
        }

        GameManager.Instance.SetFlag(doneFlag);
        if (reward == "drain")
            GameManager.Instance.RewardDrain();
        else if (reward == "stamina")
            GameManager.Instance.RewardStamina();
        else if (reward == "battery")
            GameManager.Instance.GiveBattery(1f);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast(doneToast);
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();
    }

    bool Ready()
    {
        if (string.IsNullOrEmpty(requires))
            return true;
        string[] parts = requires.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim();
            if (part.Length == 0)
                continue;
            if (!GameManager.Instance.HasFlag(part))
                return false;
        }
        return true;
    }
}
