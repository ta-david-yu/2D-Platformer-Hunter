using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasHealth
{
    CharacterStat MaxHealth { get; }

    CharacterStat HealthRecovery { get; }

    float Health { get; }
}
