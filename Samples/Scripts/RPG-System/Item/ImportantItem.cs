using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImportantItem : BaseItem
{
    public override ItemType ItemType { get { return ItemType.Important; } }
}
