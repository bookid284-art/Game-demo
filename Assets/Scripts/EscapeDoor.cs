using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeDoor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private string winSceneName = "";

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
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            return;
        }

        if (hit.collider.gameObject != gameObject)
        {
            return;
        }

        PlayerInventory inventory = playerCamera.GetComponentInParent<PlayerInventory>();
        if (inventory == null || !inventory.HasKey)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
            return;
        }

        Debug.Log("You escaped the hospital.");
    }
}
