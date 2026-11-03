using UnityEngine;

/// <summary>
/// 从护士眼前沿视线看。准星跟这条线在画面上的位置走，拾取距离仍是 3.4 米。
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    public Camera viewCamera;
    public PlayerInventory inventory;
    public float distance = 3.4f;

    IInteractable current;
    static readonly RaycastHit[] Hits = new RaycastHit[24];

    public string CurrentPrompt
    {
        get { return current != null ? current.Prompt : string.Empty; }
    }

    public Vector2 AimViewport { get; private set; }

    void Awake()
    {
        AimViewport = new Vector2(0.5f, 0.5f);
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        current = null;
        AimViewport = new Vector2(0.5f, 0.5f);
        if (GameManager.Instance != null && GameManager.Instance.BlocksPlayer)
            return;
        if (viewCamera == null)
            return;

        ThirdPersonController motor = GetComponent<ThirdPersonController>();
        Vector3 eye = motor != null ? motor.EyePosition : transform.position + Vector3.up * 1.52f;
        Vector3 look = motor != null ? motor.LookDirection : viewCamera.transform.forward;
        if (look.sqrMagnitude < 0.001f)
            look = viewCamera.transform.forward;
        Ray ray = new Ray(eye, look);
        int count = Physics.RaycastNonAlloc(
            ray,
            Hits,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            int nearest = i;
            for (int j = i + 1; j < count; j++)
            {
                if (Hits[j].distance < Hits[nearest].distance)
                    nearest = j;
            }
            RaycastHit swap = Hits[i];
            Hits[i] = Hits[nearest];
            Hits[nearest] = swap;
        }

        Vector3 focus = eye + look * 2.4f;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = Hits[i];
            if (hit.collider == null)
                continue;
            if (hit.collider.GetComponentInParent<ThirdPersonController>() != null)
                continue;

            focus = hit.point;
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && !string.IsNullOrEmpty(interactable.Prompt))
                current = interactable;
            break;
        }

        Vector3 viewport = viewCamera.WorldToViewportPoint(focus);
        if (viewport.z > 0.1f)
        {
            AimViewport = new Vector2(
                Mathf.Clamp(viewport.x, 0.12f, 0.88f),
                Mathf.Clamp(viewport.y, 0.12f, 0.88f));
        }

        if (current == null)
            return;
        if (GameManager.Instance != null && GameManager.Instance.ConsumeInteractThisFrame)
            return;
        if (Input.GetKeyDown(KeyCode.E))
            current.Interact(this);
    }
}
