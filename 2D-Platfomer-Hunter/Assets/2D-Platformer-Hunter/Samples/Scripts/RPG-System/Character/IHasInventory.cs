using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasInventory
{
    Inventory Inventory { get; }

    int AddItem(BaseItem item, int count);
    bool RemoveItem(BaseItem item, int count);

    bool HasItem(BaseItem item, int count);
}
