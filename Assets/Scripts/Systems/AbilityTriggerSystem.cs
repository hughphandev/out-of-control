using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(PhysicsInitializeGroup))]
// [UpdateBefore(typeof(PhysicsSimulationGroup))]
public partial struct AbilityTriggerSystem : ISystem
{
    void OnCreate(ref SystemState state)
    {

    }

    void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;

        new AbilityTriggerMoveJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();

        new AbilityTriggerCollisionJob()
        {
            ecb = ecb,
            physicsWorld = physicsWorld,
            resources = state.GetComponentLookup<CharacterResourceComponent>(true),
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();

        new AbilityTriggerJob()
        {
            ecb = ecb,
            physicsWorld = physicsWorld,
            resources = state.GetComponentLookup<CharacterResourceComponent>(true),
            deltaTime = SystemAPI.Time.DeltaTime,

        }.ScheduleParallel();


        // foreach ((var abilityTrigger, var transform, var physicVel, var entity) in SystemAPI.Query<RefRO<AbilityTriggerComponent>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        // {
        //     AbilityComponent ability = abilityTrigger.ValueRO.ability;
        //     if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.ValueRO.Position, abilityTrigger.ValueRO.origin) > ability.range)
        //     {
        //         ecb.DestroyEntity(entity);
        //     }

        //     var filter = Utils.LayerMaskToFilter(abilityTrigger.ValueRO.mask, abilityTrigger.ValueRO.mask);

        //     if (abilityTrigger.ValueRO.ability.autoAim)
        //     {
        //         NativeList<ColliderCastHit> aimHits = new NativeList<ColliderCastHit>(Allocator.Temp);

        //         if (physicsWorld.SphereCastAll(transform.ValueRO.Position, ability.range, float3.zero, 1, ref aimHits, filter))
        //         {
        //             var forward = Vector3.RotateTowards(transform.ValueRO.Forward(), aimHits[0].Position - transform.ValueRO.Position, math.PI * SystemAPI.Time.DeltaTime, 0);
        //             transform.ValueRW.Rotation = quaternion.LookRotation(forward, Vector3.up);
        //         }
        //         aimHits.Dispose();
        //     }

        //     float3 size = ability.hitboxSize;
        //     var radius = math.max(size.x, math.max(size.y, size.z)) / 2;
        //     NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
        //     switch (ability.hitboxShape)
        //     {
        //         case HitboxShape.Box:
        //             physicsWorld.BoxCastAll(transform.ValueRO.Position, transform.ValueRO.Rotation, ability.hitboxSize / 2, transform.ValueRO.Forward(), 1, ref hits, filter);
        //             break;
        //         case HitboxShape.Sphere:
        //             physicsWorld.SphereCastAll(transform.ValueRO.Position, radius, float3.zero, 1, ref hits, filter);
        //             break;
        //         default:
        //             Debug.LogError("Hitbox Shape Unhandled!");
        //             break;
        //     }
        //     if (hits.Length > 0)
        //     {
        //         if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnHit))
        //         {
        //             ecb.DestroyEntity(entity);
        //         }
        //         foreach (var hit in hits)
        //         {
        //             if (SystemAPI.HasComponent<CharacterResourceComponent>(hit.Entity))
        //             {
        //                 var resource = SystemAPI.GetComponent<CharacterResourceComponent>(hit.Entity);
        //                 resource.TakeDamage(ability.damage, ability.elemental);
        //                 SystemAPI.SetComponent<CharacterResourceComponent>(hit.Entity, resource);
        //             }
        //         }
        //     }
        //     physicVel.ValueRW.Linear = math.mul(transform.ValueRO.Rotation, ability.velocity);
        //     hits.Dispose();
        // }
    }
}

[BurstCompile]
public partial struct AbilityTriggerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public ComponentLookup<CharacterResourceComponent> resources;
    public float deltaTime;

    public void Execute(in AbilityTriggerComponent abilityTrigger, ref LocalTransform transform, in Entity entity, [EntityIndexInQuery] int sortKey)
    {
        AbilityComponent ability = abilityTrigger.ability;
        if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.Position, abilityTrigger.origin) > ability.range)
        {
            ecb.DestroyEntity(sortKey, entity);
        }

        var filter = Utils.LayerMaskToFilter(abilityTrigger.mask, abilityTrigger.mask);

        if (abilityTrigger.ability.autoAim)
        {
            NativeList<ColliderCastHit> aimHits = new NativeList<ColliderCastHit>(Allocator.TempJob);

            if (physicsWorld.SphereCastAll(transform.Position, ability.range, float3.zero, 1, ref aimHits, filter))
            {
                var forward = Vector3.RotateTowards(transform.Forward(), aimHits[0].Position - transform.Position, math.PI * deltaTime, 0);
                transform.Rotation = quaternion.LookRotation(forward, Vector3.up);
            }
            aimHits.Dispose();
        }
    }
}

[BurstCompile]
public partial struct AbilityTriggerMoveJob : IJobEntity
{
    public float deltaTime;

    public void Execute(in AbilityTriggerComponent abilityTrigger, ref LocalTransform transform, ref PhysicsVelocity physicVel)
    {
        physicVel.Linear = math.mul(transform.Rotation, abilityTrigger.ability.velocity);
    }
}

[BurstCompile]
public partial struct AbilityTriggerCollisionJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public PhysicsWorld physicsWorld;
    [ReadOnly] public ComponentLookup<CharacterResourceComponent> resources;
    public float deltaTime;

    public void Execute(in AbilityTriggerComponent abilityTrigger, ref LocalTransform transform, in Entity entity, [EntityIndexInQuery] int sortKey)
    {
        AbilityComponent ability = abilityTrigger.ability;
        float3 size = ability.hitboxSize;
        var radius = math.max(size.x, math.max(size.y, size.z)) / 2;
        var filter = Utils.LayerMaskToFilter(abilityTrigger.mask, abilityTrigger.mask);
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        switch (ability.hitboxShape)
        {
            case HitboxShape.Box:
                physicsWorld.BoxCastAll(transform.Position, transform.Rotation, ability.hitboxSize / 2, transform.Forward(), 1, ref hits, filter);
                break;
            case HitboxShape.Sphere:
                physicsWorld.SphereCastAll(transform.Position, radius, float3.zero, 1, ref hits, filter);
                break;
            default:
                Debug.LogError("Hitbox Shape Unhandled!");
                break;
        }
        if (hits.Length > 0)
        {
            if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnHit))
            {
                ecb.DestroyEntity(sortKey, entity);
            }
            foreach (var hit in hits)
            {
                var resource = resources[hit.Entity];
                resource.TakeDamage(ability.damage, ability.elemental);
                ecb.SetComponent<CharacterResourceComponent>(sortKey, hit.Entity, resource);
            }
        }
        hits.Dispose();
    }
}