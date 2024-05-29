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
        var grids = SystemAPI.GetSingleton<WorldGridComponent>().grids;
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