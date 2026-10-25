using System.Collections;
using UnityEngine;

/// <summary>
/// 平开门。requiredKeyId 为空表示不需要钥匙。打开后通知导航图。
/// </summary>
public class DoorController : MonoBehaviour, IInteractable
{
    public string doorId = "nurse";
    public string requiredKeyId = "";
    public string lockedHint = "这扇门锁着";
    public bool consumeKey = true;
    public Transform pivot;
    public float openAngle = 100f;
    public Collider[] blockers;
    public HospitalNavGraph nav;
    public int navA = -1;
    public int navB = -1;

    bool isOpen;
    bool busy;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    public string Prompt
    {
        get
        {
            if (isOpen || busy)
                return string.Empty;
            if (string.IsNullOrEmpty(requiredKeyId))
                return "按 E 开门";
            return "按 E 使用钥匙开门";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (isOpen || busy)
            return;

        if (!string.IsNullOrEmpty(requiredKeyId))
        {
            PlayerInventory inventory = interactor != null ? interactor.inventory : null;
            if (inventory == null || !inventory.HasKey(requiredKeyId))
            {
                if (GameHUD.Instance != null)
                    GameHUD.Instance.Toast(lockedHint);
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayLocked();
                return;
            }
            if (consumeKey)
                inventory.ConsumeKey(requiredKeyId);
        }

        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        busy = true;
        SetBlockers(false);
        if (nav != null && navA >= 0 && navB >= 0)
            nav.SetOpen(navA, navB, true);
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayDoor();

        Quaternion from = pivot != null ? pivot.localRotation : Quaternion.identity;
        Quaternion to = Quaternion.Euler(0f, openAngle, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.65f;
            if (pivot != null)
                pivot.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }

        isOpen = true;
        busy = false;
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyDoorOpened(doorId);
    }

    void SetBlockers(bool enabled)
    {
        if (blockers == null)
            return;
        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] != null)
                blockers[i].enabled = enabled;
        }
    }
}
