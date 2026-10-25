using UnityEngine;

/// <summary>
/// 从屏幕中心发射线，按 E 与钥匙、门、笔记交互。
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    public Camera viewCamera;
    public PlayerInventory inventory;
    public float distance = 3.4f;

    IInteractable current;

    public string CurrentPrompt
    {
        get { return current != null ? current.Prompt : string.Empty; }
    }

    void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        current = null;
        if (GameManager.Instance != null && GameManager.Instance.BlocksPlayer)
            return;
        if (viewCamera == null)
            return;

        Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            current = hit.collider.GetComponentInParent<IInteractable>();

        if (current != null && string.IsNullOrEmpty(current.Prompt))
            current = null;

        if (current == null)
            return;
        if (GameManager.Instance != null && GameManager.Instance.ConsumeInteractThisFrame)
            return;
        if (Input.GetKeyDown(KeyCode.E))
            current.Interact(this);
    }
}
