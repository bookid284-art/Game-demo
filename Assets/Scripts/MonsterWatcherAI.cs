using UnityEngine;

public class MonsterWatcherAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private float viewThreshold = 0.6f;

    private Camera playerCamera;


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Start()
    {
        if (target == null && Camera.main != null)
        {
            target = Camera.main.transform;
        }

        playerCamera = Camera.main;
    }

    private void Update()
    {
        if (target == null || playerCamera == null)
        {
            return;
        }

        if (IsVisibleToPlayer())
        {
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude <= stopDistance)
        {
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private bool IsVisibleToPlayer()
    {
        Vector3 toMonster = (transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(playerCamera.transform.forward, toMonster);
        if (dot < viewThreshold)
        {
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, toMonster);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return false;
        }

        return hit.transform == transform || hit.transform.IsChildOf(transform);
    }
}
