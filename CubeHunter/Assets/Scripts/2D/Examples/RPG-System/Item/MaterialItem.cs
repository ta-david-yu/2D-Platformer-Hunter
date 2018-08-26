using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialItem : BaseItem
{
    public override ItemType ItemType { get { return ItemType.Material; } }
}
