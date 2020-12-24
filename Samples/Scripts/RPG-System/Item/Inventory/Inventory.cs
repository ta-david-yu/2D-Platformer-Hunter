using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    protected int Capacity = int.MaxValue;
    
    public bool IsFull { get { return m_ItemSlots.Count >= Capacity; } }

    protected List<InventoryItemSlot> m_ItemSlots = new List<InventoryItemSlot>();

    public int AddItem(BaseItem item, int count)
    {
        int left = count;

        var slot = findItemSlot(item);
        if (slot == null)
        {
            if (!IsFull)
            {
                slot = new InventoryItemSlot(item.ItemType, item.ItemID, count);
                m_ItemSlots.Add(slot);

                left = 0;
            }
        }
        else
        {
            int sum = slot.Count + count;
            if (sum > item.MaxCount)
            {
                left = sum - item.MaxCount;
            }
            slot.Count += (count - left);
        }

        return left;
    }

    public bool RemoveItem(BaseItem item, int count)
    {
        var slot = findItemSlot(item);
        if (slot != null)
        {
            slot.Count -= count;
            if (slot.Count <= 0)
            {
                m_ItemSlots.Remove(slot);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasItem(BaseItem item, int count)
    {
        var slot = findItemSlot(item);
        if (slot != null)
        {
            if(slot.Count >= count)
            {
                return true;
            }
        }
        return false;
    }

    private InventoryItemSlot findItemSlot(BaseItem item)
    {
        foreach (var slot in m_ItemSlots)
        {
            if (slot.ItemType == item.ItemType)
            {
                if (slot.ItemID == item.ItemID)
                {
                    return slot;
                }
            }
        }
        return null;
    }
}
