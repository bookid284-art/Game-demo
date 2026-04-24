using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 2f;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            return;
        }

        if (hit.collider.gameObject != gameObject)
        {
            return;
        }

        PlayerInventory inventory = playerCamera.GetComponentInParent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        inventory.CollectKey();
        gameObject.SetActive(false);
    }
}
