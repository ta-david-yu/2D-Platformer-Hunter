using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Material,
    Usable,
    Equippable,
    Important
}

[System.Serializable]
public abstract class BaseItem
{
    public abstract ItemType ItemType { get; }

    [SerializeField]
    protected int m_ItemID;
    public int ItemID { get { return m_ItemID; } }

    [SerializeField]
    protected string m_Name;
    public string Name { get { return m_Name; } }

    [SerializeField]
    protected Sprite m_Icon;
    public Sprite Icon { get { return m_Icon; } }

    [SerializeField]
    protected string m_Description;
    public string Description { get { return m_Description; } }

    [SerializeField]
    protected int m_MaxCount = 999;
    public int MaxCount { get { return m_MaxCount; } }
}
