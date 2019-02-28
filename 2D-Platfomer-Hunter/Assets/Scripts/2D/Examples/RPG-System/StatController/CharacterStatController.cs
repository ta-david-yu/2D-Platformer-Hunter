using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatController : MonoBehaviour, IHasStat
{
    public CharacterStat MaxHealth { get; protected set; } = new CharacterStat();
    public CharacterStat HealthRecovery { get; protected set; } = new CharacterStat();

    public float Health { get; set; }

    public CharacterStat MaxMainStamina { get; protected set; } = new CharacterStat();
    public CharacterStat MaxSubStamina { get; protected set; } = new CharacterStat();

    public float MainStamina { get; set; } = new float();
    public float SubStamina { get; set; } = new float();
    
    public CharacterStat PhysicalDamage { get; protected set; } = new CharacterStat();
    public CharacterStat MagicalDamage { get; protected set; } = new CharacterStat();
    public CharacterStat PhysicalResistence { get; protected set; } = new CharacterStat();
    public CharacterStat MagicalResistence { get; protected set; } = new CharacterStat();

    public CharacterStat PhysicalDurability { get; protected set; } = new CharacterStat();
    public CharacterStat MagicalDurability { get; protected set; } = new CharacterStat();

    public CharacterStat Stability { get; protected set; } = new CharacterStat();
    public CharacterStat JumpCount { get; protected set; } = new CharacterStat(1);
    public int JumpCounter { get; set; } = new int(); 
    public CharacterStat CanWallJump { get; protected set; } = new CharacterStat();

    public bool Equip(EquippableItem equipment)
    {
        throw new NotImplementedException();
    }

    public bool Unequip(EquippableItem equipment)
    {
        throw new NotImplementedException();
    }
}
