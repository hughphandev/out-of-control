using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class AbilityTriggerAuthoring : AutoAuthoring<AbilityTriggerComponent> { }

[Serializable]
public struct AbilityTriggerComponent : IComponentData
{
    [HideInInspector] public LayerMask mask;
    [HideInInspector] public Vector3 origin;
    [HideInInspector] public AbilityComponent ability;
    // [HideInInspector] public Extention[] extentions;
    // [HideInInspector] public List<IDamagable> hitted = new List<IDamagable>();
    // public List<AbilityTriggerComponent> triggers;
}
