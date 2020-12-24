using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    HealthRecovery,

    MaxMainStamina,
    MaxSubStamina,

    PhysicalDamage,
    MagicalDamage,

    PhysicalResistence,
    MagicalResistence,

    PhysicalDurability,
    MagicalDurability,

    Stability,
    JumpCount,
    CanWallJump
}

[System.Serializable]
public class CharacterStat
{
    protected float m_BaseValue;
    public float BaseValue { get { return m_BaseValue; } set { m_BaseValue = value; } }
    
    protected float m_PrevBaseValue;
    protected bool m_IsDirty = true;

    protected float m_Value;
    public float Value
    {
        get
        {
            if (m_IsDirty || m_PrevBaseValue != BaseValue)
            {
                m_PrevBaseValue = BaseValue;

                m_Value = CalculateFinalValue();

                m_IsDirty = false;
            }

            return m_Value;
        }
    }

    public bool BoolValue
    {
        get
        {
            return Value > 0;
        }
    }

    protected readonly List<StatModifier> m_StatModifiers;
    public readonly ReadOnlyCollection<StatModifier> StatModifiers;

    public CharacterStat()
    {
        m_StatModifiers = new List<StatModifier>();
        StatModifiers = m_StatModifiers.AsReadOnly();
    }

    public CharacterStat(float bValue) : this()
    {
        m_BaseValue = bValue;
    }

    public virtual void AddModifier(StatModifier mod)
    {
        m_IsDirty = true;
        m_StatModifiers.Add(mod);
        m_StatModifiers.Sort(CompareModifierOrder);
    }


    public virtual bool RemoveModifier(StatModifier mod)
    {
        if (m_StatModifiers.Remove(mod))
        {
            m_IsDirty = true;
            return true;
        }
        return false;
    }

    public virtual bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;

        for (int i = m_StatModifiers.Count - 1; i >= 0; i--)
        {
            if (m_StatModifiers[i].Source == source)
            {
                m_IsDirty = true;
                didRemove = true;
                m_StatModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

    protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Order < b.Order)
            return -1;
        else if (a.Order > b.Order)
            return 1;
        return 0; //if (a.Order == b.Order)
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < m_StatModifiers.Count; i++)
        {
            StatModifier mod = m_StatModifiers[i];
            
            if (mod.ModType == StatModifyType.Toggle)
            {
                finalValue = mod.Value;
                break;
            }
            else if (mod.ModType == StatModifyType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.ModType == StatModifyType.PercentAdd)
            {
                sumPercentAdd += mod.Value;

                if (i + 1 >= m_StatModifiers.Count || m_StatModifiers[i + 1].ModType != StatModifyType.PercentAdd)
                {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0;
                }
            }
            else if (mod.ModType == StatModifyType.PercentMul)
            {
                finalValue *= 1 + mod.Value;
            }
        }

        // Workaround for float calculation errors, like displaying 12.00002 instead of 12
        return (float)Math.Round(finalValue, 4);
    }
}
