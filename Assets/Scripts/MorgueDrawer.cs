using UnityEngine;

/// <summary>
/// 太平间抽屉。只有腕带相符的那一格给出安静室钥匙。
/// </summary>
public class MorgueDrawer : MonoBehaviour, IInteractable
{
    public string label = "000-00";
    public bool correct;

    bool emptied;

    public string Prompt
    {
        get
        {
            if (emptied)
                return string.Empty;
            return "按 E 拉开腕带 " + label;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (emptied)
            return;
        if (!correct)
        {
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast("不是这具");
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayLocked();
            return;
        }

        emptied = true;
        if (interactor != null && interactor.inventory != null)
            interactor.inventory.AddKey("quiet", "安静室钥匙");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyKeyCollected("quiet");
            GameManager.Instance.SetFlag("morgue_list");
        }
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("名册对上了，夹层里有安静室钥匙");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();
    }
}
