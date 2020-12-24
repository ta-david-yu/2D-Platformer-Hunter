using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "WeaponItemList", menuName = "GameData/WeaponItemList")]
public class WeaponItemList : ScriptableObject
{
    public WeaponItem this[int index]
    {
        get
        {
            return m_Items[index];
        }
    }

    [SerializeField]
    private List<WeaponItem> m_Items;
}