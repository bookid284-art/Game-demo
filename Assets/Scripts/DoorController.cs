using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private bool requiresKey = true;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private Transform doorPanel;
    [SerializeField] private Vector3 openedRotation = new Vector3(0f, -100f, 0f);

    private Quaternion closedRotation;
    private bool isOpen;

    private void Awake()
    {
        if (doorPanel == null)
        {
            doorPanel = transform;
        }

        closedRotation = doorPanel.localRotation;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E) || isOpen)
        {
            return;
        }

        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            return;
        }

        if (hit.collider.transform != transform && hit.collider.transform != doorPanel)
        {
            return;
        }

        if (requiresKey)
        {
            PlayerInventory inventory = playerCamera.GetComponentInParent<PlayerInventory>();
            if (inventory == null || !inventory.HasKey)
            {
                return;
            }
        }

        OpenDoor();
    }

    public void OpenDoor()
    {
        isOpen = true;
        doorPanel.localRotation = closedRotation * Quaternion.Euler(openedRotation);
    }
}
