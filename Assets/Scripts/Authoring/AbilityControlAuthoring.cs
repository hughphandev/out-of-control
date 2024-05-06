using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using AutoAuthoring;
using Unity.Transforms;

public class AbilityControlAuthoring : MonoBehaviour
{
    public Transform attackTransform;
    public LayerMask damageMask;
    public AbilityComponent currentAbility;
    public List<AbilityComponent> abilities = new List<AbilityComponent>();
    public GameObject abilityTriggerPrefab;

    class Baker : Baker<AbilityControlAuthoring>
    {
        public override void Bake(AbilityControlAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AbilityControlComponent()
            {
                attackTransform = GetEntity(authoring.attackTransform.gameObject, TransformUsageFlags.Dynamic),
                damageMask = authoring.damageMask,
                currentAbility = authoring.currentAbility,
                abilityTriggerPrefab = GetEntity(authoring.abilityTriggerPrefab, TransformUsageFlags.Dynamic),
            });

            var buffer = AddBuffer<AbilityBufferElement>(entity);
            foreach (var ability in authoring.abilities)
            {
                buffer.Add(new AbilityBufferElement()
                {
                    value = ability
                });
            }
        }
    }
}

[Serializable]
public struct AbilityControlComponent : IComponentData
{
    public Entity attackTransform;
    public LayerMask damageMask;
    public float3 targetPosition;
    public float timer;
    public AbilityComponent currentAbility;
    public Entity abilityTriggerPrefab;
}
