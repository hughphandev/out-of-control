using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityAuthoring : AutoAuthoring<AbilityComponent> { }

[Serializable]
public struct AbilityComponent : IComponentData
{
    public AbilityTag tag;
    public float coolDown;
    public int damage;
    public int shield;
    public int projectileCount;
    public int chainCount;
    public float lifeTime;
    public float range;
    public bool parent;
    // public bool allowAnimation;
    public bool autoAim;
    // public AnimationClip animation;
    // public AnimationProperty animationProperty;
    public AbilityDestroyFlag destroyFlag;
    public AbilitySpawnLocation spawnLocation;
    public Elemental elemental;
    public HitboxShape hitboxShape;
    public Vector3 hitboxSize;
    public float3 velocity;
    public float3 offset;
    // public AudioClip soundEffect;
    // public AbilitySpawnable spawnOnDestroy;
    public float3 spawnOnDestroySize;
    public int spawnOnDestroyDamage;
    // public NativeArray<Entity> elementals;
}

[ChunkSerializable]
public struct AbilityRuntimeBufferElement : IBufferElementData
{
    public AbilityComponent value;
    public ElementalVFX elementalsPrefabs;
    public float timer;

    public AbilityRuntimeBufferElement(ref AbilityRuntimeBufferElement value)
    {
        this = value;
    }
}
