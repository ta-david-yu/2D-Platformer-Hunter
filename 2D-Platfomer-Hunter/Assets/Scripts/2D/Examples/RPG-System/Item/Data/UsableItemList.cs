using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "UsableItemList", menuName = "GameData/UsableItemList")]
public class UsableItemList : ScriptableObject
{
    public UsableItem this[int index]
    {
        get
        {
            return m_Items[index];
        }
    }

    [SerializeField]
    private List<UsableItem> m_Items;
}
