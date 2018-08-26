using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UsableItem : BaseItem
{
    public override ItemType ItemType { get { return ItemType.Usable; } }
}
