using UnityEngine;

/// <summary>
/// 疏散终端。前置标记都满足时盖章。
/// </summary>
public class StampTerminal : MonoBehaviour, IInteractable
{
    public string stampId = "stamp_er";
    public string requires = "";
    public string lockedHint = "条件还没齐";

    public string Prompt
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.HasFlag(stampId))
                return string.Empty;
            return "按 E 在终端盖章";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance == null || GameManager.Instance.HasFlag(stampId))
            return;
        if (!Ready())
        {
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast(lockedHint);
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayLocked();
            return;
        }
        GameManager.Instance.SetStamp(stampId);
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
