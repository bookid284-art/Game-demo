using System.Collections;
using UnityEngine;

/// <summary>
/// 紧急出口闸门。需要出口钥匙，打开后向上升起。
/// </summary>
public class EscapeDoor : MonoBehaviour, IInteractable
{
    public string doorId = "gate";
    public string requiredKeyId = "exit";
    public string lockedHint = "紧急出口锁着，需要出口钥匙";
    public string openedToast = "出口开了，快出去";
    public bool consumeKey = true;
    public Transform panel;
    public Collider[] blockers;
    public HospitalNavGraph nav;
    public int navA = -1;
    public int navB = -1;
    public float slideDistance = 2.7f;

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
            return "按 E 打开紧急出口";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (isOpen || busy)
            return;

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
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyDoorOpened(doorId);

        Vector3 from = panel != null ? panel.localPosition : Vector3.zero;
        Vector3 to = from + Vector3.up * slideDistance;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.1f;
            if (panel != null)
                panel.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }

        isOpen = true;
        busy = false;
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast(openedToast);
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
