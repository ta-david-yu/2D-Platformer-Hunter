using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType
{
    Head,
    Body,
    Foot,
    Accessory,

    MainWeapon,
    SubWeapon
}

[System.Serializable]
public class EquippableItem : BaseItem
{
    public override ItemType ItemType { get { return ItemType.Equippable; } }

    [SerializeField]
    protected EquipmentType m_EquipmentType;
    public EquipmentType EquipmentType { get { return m_EquipmentType; } }

    [SerializeField]
    protected List<StatModifierContainer> m_ModifierContainers;

    public void Equip(IHasStat stat)
    {
        foreach (var mod in m_ModifierContainers)
        {
            StatModifier modifier = new StatModifier(mod.Value, mod.ModType, mod.Order, this);

            switch (mod.StatType)
            {
                case StatType.MaxHealth:
                    stat.MaxHealth.AddModifier(modifier);
                    break;
                case StatType.HealthRecovery:
                    stat.HealthRecovery.AddModifier(modifier);
                    break;

                case StatType.MaxMainStamina:
                    stat.MaxMainStamina.AddModifier(modifier);
                    break;
                case StatType.MaxSubStamina:
                    stat.MaxSubStamina.AddModifier(modifier);
                    break;

                case StatType.PhysicalDamage:
                    stat.PhysicalDamage.AddModifier(modifier);
                    break;
                case StatType.MagicalDamage:
                    stat.MagicalDamage.AddModifier(modifier);
                    break;

                case StatType.PhysicalResistence:
                    stat.PhysicalResistence.AddModifier(modifier);
                    break;
                case StatType.MagicalResistence:
                    stat.MagicalResistence.AddModifier(modifier);
                    break;

                case StatType.PhysicalDurability:
                    stat.PhysicalDurability.AddModifier(modifier);
                    break;
                case StatType.MagicalDurability:
                    stat.MagicalDurability.AddModifier(modifier);
                    break;

                case StatType.Stability:
                    stat.Stability.AddModifier(modifier);
                    break;
                case StatType.JumpCount:
                    stat.JumpCount.AddModifier(modifier);
                    break;
                case StatType.CanWallJump:
                    stat.CanWallJump.AddModifier(modifier);
                    break;
            }
        }
    }

    public void Unequip(IHasStat stat)
    {
        foreach (var mod in m_ModifierContainers)
        {
            switch (mod.StatType)
            {
                case StatType.MaxHealth:
                    stat.MaxHealth.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.HealthRecovery:
                    stat.HealthRecovery.RemoveAllModifiersFromSource(this);
                    break;

                case StatType.MaxMainStamina:
                    stat.MaxMainStamina.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.MaxSubStamina:
                    stat.MaxSubStamina.RemoveAllModifiersFromSource(this);
                    break;

                case StatType.PhysicalDamage:
                    stat.PhysicalDamage.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.MagicalDamage:
                    stat.MagicalDamage.RemoveAllModifiersFromSource(this);
                    break;

                case StatType.PhysicalResistence:
                    stat.PhysicalResistence.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.MagicalResistence:
                    stat.MagicalResistence.RemoveAllModifiersFromSource(this);
                    break;

                case StatType.PhysicalDurability:
                    stat.PhysicalDurability.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.MagicalDurability:
                    stat.MagicalDurability.RemoveAllModifiersFromSource(this);
                    break;

                case StatType.Stability:
                    stat.Stability.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.JumpCount:
                    stat.JumpCount.RemoveAllModifiersFromSource(this);
                    break;
                case StatType.CanWallJump:
                    stat.CanWallJump.RemoveAllModifiersFromSource(this);
                    break;
            }
        }
    }
}

[System.Serializable]
public class WeaponItem : EquippableItem
{
    // TODO: add skill module
}

