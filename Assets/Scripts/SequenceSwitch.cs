using UnityEngine;

/// <summary>
/// 顺序阀门。扳错会整组弹回。
/// </summary>
public class SequenceSwitch : MonoBehaviour, IInteractable
{
    public string groupId = "valve";
    public string token = "白";

    public string Prompt
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.HasFlag(groupId == "valve" ? "valves" : groupId))
                return string.Empty;
            return "按 E 扳动" + token + "阀";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PushToken(groupId, token);
    }
}
