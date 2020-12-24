using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItemSlot
{
    public ItemType ItemType;

    public int ItemID;

    public int Count;

    public InventoryItemSlot(ItemType type, int id, int count)
    {
        ItemType = type;
        ItemID = id;
        Count = count;
    }
}
