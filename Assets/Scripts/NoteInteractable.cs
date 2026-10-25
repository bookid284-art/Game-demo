using UnityEngine;

/// <summary>
/// 可阅读的病历或交接纸条。阅读时玩家不能移动，怪物仍会行动。
/// </summary>
public class NoteInteractable : MonoBehaviour, IInteractable
{
    public string title = "记录";
    [TextArea(3, 8)]
    public string body = "";

    public string Prompt
    {
        get { return "按 E 阅读" + title; }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BeginReading();
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowNote(title, body);
    }
}
