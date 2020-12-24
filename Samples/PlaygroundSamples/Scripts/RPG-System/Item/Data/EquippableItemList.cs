using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "ArmourItemList", menuName = "GameData/ArmourItemList")]
public class EquippableItemList : ScriptableObject
{
    public EquippableItem this[int index]
    {
        get
        {
            return m_Items[index];
        }
    }

    [SerializeField]
    private List<EquippableItem> m_Items;
}
