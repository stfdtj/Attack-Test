using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transmission : MonoBehaviour
{
    public Inventory inventory;
    public BroadCast feed;
    public string playerName = "Player";

    void OnEnable() { if (inventory) inventory.OnItemAdded += OnItemAdded; }
    void OnDisable() { if (inventory) inventory.OnItemAdded -= OnItemAdded; }

    void OnItemAdded(Item item, int amount)
    {
        if (feed) feed.AddLoot(playerName, item, amount);
        else Debug.LogWarning("LootBroadcastBridge: feed not assigned");
    }
}
