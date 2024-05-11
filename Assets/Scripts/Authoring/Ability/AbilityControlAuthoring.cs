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
    // public AbilityComponent currentAbility;
    public List<AbilityComponent> abilities = new List<AbilityComponent>();
    public GameObject abilityTriggerPrefab;

    class Baker : Baker<AbilityControlAuthoring>
    {
        public override void Bake(AbilityControlAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var abilities = AddBuffer<AbilityRuntimeBufferElement>(entity);
            foreach (var ability in authoring.abilities)
            {
                abilities.Add(new AbilityRuntimeBufferElement()
                {
                    value = ability
                });
            }

            AddComponent(entity, new AbilityControlComponent()
            {
                attackTransform = GetEntity(authoring.attackTransform.gameObject, TransformUsageFlags.Dynamic),
                damageMask = authoring.damageMask,
                // currentAbility = new AbilityRuntimeBufferElement() { value = authoring.currentAbility },
                abilityTriggerPrefab = GetEntity(authoring.abilityTriggerPrefab, TransformUsageFlags.Dynamic),
            });

        }
    }
}

[Serializable]
public struct AbilityControlComponent : IComponentData
{
    public Entity attackTransform;
    public LayerMask damageMask;
    public float3 targetPosition;
    public AbilityRuntimeBufferElement currentAbility;
    public Entity abilityTriggerPrefab;
}
