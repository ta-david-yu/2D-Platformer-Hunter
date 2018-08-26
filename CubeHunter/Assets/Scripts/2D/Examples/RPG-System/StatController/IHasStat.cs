using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasStat : IHasHealth, IHasStamina
{
    CharacterStat PhysicalDamage { get; }
    CharacterStat MagicalDamage { get; }
    CharacterStat PhysicalResistence { get; }
    CharacterStat MagicalResistence { get; }

    CharacterStat Stability { get; }            // Stability, Blow off endurance

    CharacterStat JumpCount { get; }            // JumpCount, Number of air jump allowed
    int JumpCounter { get; }

    CharacterStat CanWallJump { get; }          // CanWallJump
}
