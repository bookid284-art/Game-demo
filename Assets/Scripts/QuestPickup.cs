using UnityEngine;

/// <summary>
/// 支线物品。拾取后记下标记。
/// </summary>
public class QuestPickup : MonoBehaviour, IInteractable
{
    public string flagId = "vial1";
    public string displayName = "物品";

    Vector3 basePosition;

    public string Prompt
    {
        get { return "按 E 拾取" + displayName; }
    }

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        transform.position = basePosition + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.04f;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetFlag(flagId);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("拿到" + displayName);
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayPickup();
        Destroy(gameObject);
    }
}
