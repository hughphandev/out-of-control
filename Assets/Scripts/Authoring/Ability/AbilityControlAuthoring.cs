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
    public List<AbilityConfig> abilities = new List<AbilityConfig>();
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

    private void OnValidate()
    {

        for (int i = 0; i < abilities.Count; ++i)
        {
            if (abilities[i].elementalsPrefabs.Length != GameConstant.ElementalTypeCount) Array.Resize(ref abilities[i].elementalsPrefabs, GameConstant.ElementalTypeCount);
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


[Serializable]
public class AbilityConfig
{
    public AbilityComponent value;
    [LabeledArray(typeof(Elemental))] public GameObject[] elementalsPrefabs;
}