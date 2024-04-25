using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using AutoAuthoring;
using Unity.Transforms;

public class AbilityControlAuthoring : AutoAuthoring<AbilityControlComponent> { }

[Serializable]
public struct AbilityControlComponent : IComponentData
{
    public LocalTransform attackTransform;
    public LayerMask damageMask;
    public float3 targetPosition;
    public AbilityComponent currentAbility;
    public float abilityCooldown;
    public Entity abilityTriggerPrefab;
}
