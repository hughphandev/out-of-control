using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using AutoAuthoring;
using Unity.Transforms;
using Unity.Collections;

public class AbilityControlAuthoring : MonoBehaviour
{
    public Transform attackTransform;
    public LayerMask damageMask;
    // public AbilityComponent currentAbility;
    public List<AbilitySO> abilities = new List<AbilitySO>();
    public GameObject abilityTriggerPrefab;

    class Baker : Baker<AbilityControlAuthoring>
    {
        public override void Bake(AbilityControlAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var abilities = AddBuffer<AbilityRuntimeBufferElement>(entity);
            foreach (var ability in authoring.abilities)
            {
                ElementalVFX elements = new ElementalVFX();
                for (int i = 0; i < ability.elementalsPrefabs.Length; ++i)
                {
                    elements[(Elemental)i] = GetEntity(ability.elementalsPrefabs[i], TransformUsageFlags.None);
                }
                var prefabs = ability.elementalsPrefabs;
                abilities.Add(new AbilityRuntimeBufferElement()
                {
                    value = ability.value,
                    elementalsPrefabs = elements,
                });
            }

            AddComponent(entity, new AbilityControlComponent()
            {
                attackTransform = GetEntity(authoring.attackTransform.gameObject, TransformUsageFlags.Dynamic),
                damageMask = authoring.damageMask,
                // currentAbility = new AbilityRuntimeBufferElement() { value = authoring.currentAbility },
                abilityTriggerPrefab = GetEntity(authoring.abilityTriggerPrefab, TransformUsageFlags.None),
            });

        }
    }
}

[Serializable]
public struct AbilityControlComponent : IComponentData, IEnableableComponent
{
    public Entity attackTransform;
    public LayerMask damageMask;
    public float3 targetPosition;
    // public AbilityRuntimeBufferElement currentAbility;
    public Entity abilityTriggerPrefab;
}