using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasStamina
{
    CharacterStat MaxMainStamina { get; }
    CharacterStat MaxSubStamina { get; }

    CharacterStat PhysicalDurability { get; }   // PhysicalDurability, Physical Stamina recovery speed
    CharacterStat MagicalDurability { get; }    // MagicalDurability, Magical Stamina recovery speed

    float MainStamina { get; }
    float SubStamina { get; }
}
