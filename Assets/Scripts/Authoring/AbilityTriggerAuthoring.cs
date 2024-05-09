using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class AbilityTriggerAuthoring : MonoBehaviour
{
    private class Baker : Baker<AbilityTriggerAuthoring>
    {
        public override void Bake(AbilityTriggerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AbilityTriggerComponent());
        }
    }
}

[Serializable]
public struct AbilityTriggerComponent : IComponentData
{
    public LayerMask mask;
    public float3 origin;
    public AbilityComponent ability;
    // [HideInInspector] public Extention[] extentions;
    // [HideInInspector] public List<IDamagable> hitted = new List<IDamagable>();
    // public List<AbilityTriggerComponent> triggers;
}
