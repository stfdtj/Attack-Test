
using System;

[Serializable]
public struct InventorySlot
{
    public Item item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;

    public int SpaceLeftInStack => (item == null || !item.stackable) ? 0 : (item.maxStack - count);
}
