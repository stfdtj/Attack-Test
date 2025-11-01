using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{

    public int amount = 1;
    private Inventory playerInventory;

    void Awake()
    {
   
        playerInventory = GetComponentInParent<Inventory>();


        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
    
        var item = other.GetComponent<Item>();
        if (item == null || playerInventory == null) return;

        int added = playerInventory.Add(item, amount);
        if (added > 0)
        {
            // remove item
            Destroy(other.gameObject);
            Debug.Log($"Picked up {item.displayName} x{added}");
        }
    }

}
