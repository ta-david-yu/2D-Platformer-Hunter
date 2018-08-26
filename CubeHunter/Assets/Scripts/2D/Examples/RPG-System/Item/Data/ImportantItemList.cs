using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "ImportantItemList", menuName = "GameData/ImportantItemList")]
public class ImportantItemList : ScriptableObject
{
    public ImportantItem this[int index]
    {
        get
        {
            return m_Items[index];
        }
    }

    [SerializeField]
    private List<ImportantItem> m_Items;
}