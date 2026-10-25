using UnityEngine;

/// <summary>
/// 四个章齐了之后打印疏散单，并放开病房里的人。
/// </summary>
public class EvacPrinter : MonoBehaviour, IInteractable
{
    bool printed;

    public string Prompt
    {
        get
        {
            if (printed)
                return string.Empty;
            return "按 E 打印疏散单";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (printed || GameManager.Instance == null)
            return;
        if (!GameManager.Instance.HasFlag("stamp_er") || !GameManager.Instance.HasFlag("stamp_ward")
            || !GameManager.Instance.HasFlag("stamp_or") || !GameManager.Instance.HasFlag("stamp_psych"))
        {
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast("四个章还没齐，打印机不吐纸");
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayLocked();
            return;
        }

        printed = true;
        if (interactor != null && interactor.inventory != null)
            interactor.inventory.AddKey("auth", "疏散单");
        GameManager.Instance.SetFlag("auth");
        GameManager.Instance.HoldObjectiveToast();
        GameManager.Instance.ReleaseMonster();
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("疏散单印出来了。它不再停在闸门里");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();
    }
}
