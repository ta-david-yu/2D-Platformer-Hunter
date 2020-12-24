using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatModifyType
{
    Toggle = 100,
    Flat = 200,
    PercentAdd = 300,
    PercentMul = 400
}

public class StatModifier
{
    public readonly StatModifyType ModType;
    public readonly float Value;
    public readonly int Order;
    public readonly object Source;

    public StatModifier(float value, StatModifyType type, int order, object source)
    {
        Value = value;
        ModType = type;
        Order = order;
        Source = source;
    }

    public StatModifier(float value, StatModifyType type) : this(value, type, (int)type, null) { }

    public StatModifier(float value, StatModifyType type, int order) : this(value, type, order, null) { }

    public StatModifier(float value, StatModifyType type, object source) : this(value, type, (int)type, source) { }
}

[System.Serializable]
public class StatModifierContainer
{
    public StatType StatType;
    public StatModifyType ModType;
    public float Value;
    public int Order;
}
