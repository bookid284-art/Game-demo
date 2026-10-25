using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 玩家钥匙库存。钥匙用 id 区分，显示名用于 HUD。
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    readonly Dictionary<string, string> keys = new Dictionary<string, string>();

    public event Action Changed;

    public static PlayerInventory Find(PlayerInteractor interactor)
    {
        if (interactor != null && interactor.inventory != null)
            return interactor.inventory;
        if (interactor != null)
        {
            PlayerInventory onActor = interactor.GetComponent<PlayerInventory>();
            if (onActor != null)
                return onActor;
        }
        if (GameManager.Instance != null && GameManager.Instance.inventory != null)
            return GameManager.Instance.inventory;
        if (GameManager.Instance != null && GameManager.Instance.player != null)
            return GameManager.Instance.player.GetComponent<PlayerInventory>();
        return null;
    }

    public bool HasKey(string keyId)
    {
        return !string.IsNullOrEmpty(keyId) && keys.ContainsKey(keyId);
    }

    public void AddKey(string keyId, string displayName)
    {
        if (string.IsNullOrEmpty(keyId))
            return;
        keys[keyId] = string.IsNullOrEmpty(displayName) ? keyId : displayName;
        if (Changed != null)
            Changed();
    }

    public bool ConsumeKey(string keyId)
    {
        if (!HasKey(keyId))
            return false;
        keys.Remove(keyId);
        if (Changed != null)
            Changed();
        return true;
    }

    public string ListKeys()
    {
        if (keys.Count == 0)
            return "无";
        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, string> pair in keys)
        {
            if (builder.Length > 0)
                builder.Append("   ");
            builder.Append(pair.Value);
        }
        return builder.ToString();
    }
}
