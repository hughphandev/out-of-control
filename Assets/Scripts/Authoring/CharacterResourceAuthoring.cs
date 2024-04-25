using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class CharacterResourceAuthoring : AutoAuthoring<CharacterResourceComponent> { }

[Serializable]
public struct CharacterResourceComponent : IComponentData
{
    public int hp;
    public int shield;
    public int hpRecovery;
    public int experienceDrop;
    public float pickUpRange;
    public float attackSpeedMultiplier;



    public static CharacterResourceComponent operator +(CharacterResourceComponent lhs, CharacterResourceComponent rhs)
    {
        CharacterResourceComponent result = new CharacterResourceComponent();
        result.hp = lhs.hp + rhs.hp;
        result.hpRecovery = lhs.hpRecovery + rhs.hpRecovery;
        result.experienceDrop = lhs.experienceDrop + rhs.experienceDrop;
        result.pickUpRange = lhs.pickUpRange + rhs.pickUpRange;
        return result;
    }
}
