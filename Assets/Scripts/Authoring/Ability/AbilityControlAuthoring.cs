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
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var abilities = AddBuffer<AbilityRuntimeBufferElement>(entity);
            foreach (var ability in authoring.abilities)
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref ElementalVFX elements = ref builder.ConstructRoot<ElementalVFX>();

                var vfxBuilder = builder.Allocate(ref elements.values, GameConstant.ElementalTypeCount);
                var arr = new NativeArray<Entity>(GameConstant.ElementalTypeCount, Allocator.Persistent);

                for (int i = 0; i < vfxBuilder.Length; ++i)
                {
                    vfxBuilder[i] = GetEntity(ability.elementalsPrefabs[i], TransformUsageFlags.Dynamic);
                    arr[i] = GetEntity(ability.elementalsPrefabs[i], TransformUsageFlags.Dynamic);
                }
                var prefabs = ability.elementalsPrefabs;
                abilities.Add(new AbilityRuntimeBufferElement()
                {
                    value = ability.value,
                    elementalsPrefabs = builder.CreateBlobAssetReference<ElementalVFX>(Allocator.Persistent),
                    vfxPrefabs = arr,
                    vfxPrefab = GetEntity(prefabs[0], TransformUsageFlags.Dynamic)
                });
                builder.Dispose();
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