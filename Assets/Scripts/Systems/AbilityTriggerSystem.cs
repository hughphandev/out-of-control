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
    ComponentLookup<CharacterResourceComponent> resources;
    void OnCreate(ref SystemState state)
    {
        resources = state.GetComponentLookup<CharacterResourceComponent>(true);

    }

    void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
        resources.Update(ref state);

        new AbilityTriggerMoveJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();

        new AbilityTriggerCollisionJob()
        {
            ecb = ecb,
            physicsWorld = physicsWorld,
            resources = resources,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel();

        new AbilityTriggerJob()
        {
            ecb = ecb,
            physicsWorld = physicsWorld,
            resources = resources,
            deltaTime = SystemAPI.Time.DeltaTime,

        }.ScheduleParallel();
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
        if (Utils.OverlapFlag((uint)ability.destroyFlag, (uint)AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.Position, abilityTrigger.origin) > ability.range)
        {
            ecb.DestroyEntity(sortKey, entity);
        }

        var filter = Utils.LayerMaskToFilter(abilityTrigger.layer, abilityTrigger.damageMask);

        if (abilityTrigger.ability.followingProjectile)
        {
            NativeList<DistanceHit> aimHits = new NativeList<DistanceHit>(Allocator.TempJob);
            if (physicsWorld.OverlapSphere(transform.Position, ability.range, ref aimHits, filter))
            {
                var nearest = aimHits[0];
                foreach (var hit in aimHits)
                {
                    if (nearest.Distance > hit.Distance)
                    {
                        nearest = hit;
                    }
                }

                var forward = Vector3.RotateTowards(transform.Forward(), nearest.Position - transform.Position, math.PI * deltaTime, 0);
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
        var filter = Utils.LayerMaskToFilter(abilityTrigger.damageMask, abilityTrigger.damageMask);
        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);
        switch (ability.hitboxShape)
        {
            case HitboxShape.Box:
                physicsWorld.OverlapBox(transform.Position, transform.Rotation, ability.hitboxSize / 2, ref hits, filter);
                break;
            case HitboxShape.Sphere:
                physicsWorld.OverlapSphere(transform.Position, radius, ref hits, filter);
                break;
            default:
                Debug.LogError("Hitbox Shape Unhandled!");
                break;
        }
        if (hits.Length > 0)
        {
            if (Utils.OverlapFlag((uint)ability.destroyFlag, (uint)AbilityDestroyFlag.OnHit))
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

// [UpdateInGroup(typeof(LateSimulationSystemGroup))]
// public partial struct AbilityTriggerCleanUp : ISystem
// {
//     [BurstCompile]
//     void OnUpdate(ref SystemState state)
//     {
//         var query = new EntityQueryBuilder(Allocator.Temp)
//             .WithAll<Disabled>()
//             .Build(ref state);
//         state.EntityManager.DestroyEntity(query);
//     }
// }