using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasEquipment
{
    Equipment Equipment { get; }
    bool Equip(EquippableItem equipment);
    bool Unequip(EquippableItem equipment);
}

