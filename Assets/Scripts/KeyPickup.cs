using UnityEngine;

/// <summary>
/// 钥匙拾取物。拾取后交给 PlayerInventory。
/// </summary>
public class KeyPickup : MonoBehaviour, IInteractable
{
    public string keyId = "storage";
    public string displayName = "库房钥匙";

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
        transform.Rotate(0f, 50f * Time.deltaTime, 0f, Space.World);
        transform.position = basePosition + Vector3.up * Mathf.Sin(Time.time * 2.2f) * 0.06f;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor == null || interactor.inventory == null)
            return;
        interactor.inventory.AddKey(keyId, displayName);
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyKeyCollected(keyId);
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("获得" + displayName);
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();
        Destroy(gameObject);
    }
}
