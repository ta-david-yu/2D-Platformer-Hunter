using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "MaterialItemList", menuName = "GameData/MaterialItemList")]
public class MaterialItemList : ScriptableObject
{
    public MaterialItem this[int index]
    {
        get
        {
            return m_Items[index];
        }
    }

    [SerializeField]
    private List<MaterialItem> m_Items;
}