using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    
    [SerializeField] private List<Item> items = new();
    [SerializeField] private List<int> counts = new();

    // event for UI/broadcast
    public event Action<Item, int> OnItemAdded;
    public event Action<Item, int> OnItemRemoved;


    int IndexOf(Item item)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i] == item) return i;
        return -1;
    }

    // add amount
    public int Add(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return 0;

        int i = IndexOf(item);
        if (i >= 0)
        {
            counts[i] += amount;
        }
        else
        {
            items.Add(item);
            counts.Add(amount);
        }

        OnItemAdded?.Invoke(item, amount);
        return amount;
    }


    public int CountOf(Item item)
    {
        int i = IndexOf(item);
        return (i >= 0) ? counts[i] : 0;
    }

  
    public void Update()
    {
        // For testing: press I to print inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintInventory();
        }
    }
    
    public void PrintInventory()
    {
        Debug.Log($"=== InventorySimple ({GetInstanceID()}) ===");
        for (int i = 0; i < items.Count; i++)
            Debug.Log($"{items[i].displayName} x{counts[i]}");
        Debug.Log("==============================");
    }
}

    
