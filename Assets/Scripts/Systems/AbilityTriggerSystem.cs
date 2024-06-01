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
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(PhysicsInitializeGroup))]
// [UpdateBefore(typeof(PhysicsSimulationGroup))]
public partial struct AbilityTriggerSystem : ISystem
{
    ComponentLookup<CharacterResourceComponent> resources;
    ComponentLookup<LocalToWorld> l2wComponents;
    ComponentLookup<WorldCircleCollider> colliders;
    void OnCreate(ref SystemState state)
    {
        resources = state.GetComponentLookup<CharacterResourceComponent>(true);
        l2wComponents = state.GetComponentLookup<LocalToWorld>(true);
        colliders = state.GetComponentLookup<WorldCircleCollider>(true);
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
        l2wComponents.Update(ref state);
        colliders.Update(ref state);


        // foreach ((var abilityTrigger, var collider, var physicVel, var transform, var entity) in SystemAPI.Query<RefRO<AbilityTriggerComponent>, RefRO<WorldCircleCollider>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>().WithEntityAccess())
        // {
        //     AbilityComponent ability = abilityTrigger.ValueRO.ability;

        //     if (abilityTrigger.ValueRO.ability.autoAim)
        //     {
        //         NativeList<Entity> validEntities = grids.GetValidEntities(transform.ValueRO.Position, ability.range, Allocator.Temp);
        //         float3 nearest = new float3(math.INFINITY, math.INFINITY, math.INFINITY);
        //         foreach (var current in validEntities)
        //         {
        //             float3 currentPos = SystemAPI.GetComponent<LocalToWorld>(current).Position;
        //             float currentDistance = math.distance(currentPos, transform.ValueRO.Position);
        //             float nearestDistance = math.distance(nearest, transform.ValueRO.Position);
        //             if (Utils.OverlapFlag((uint)abilityTrigger.ValueRO.mask.value, (uint)SystemAPI.GetComponent<WorldCircleCollider>(current).layer.value) && current != entity && currentDistance < nearestDistance)
        //             {
        //                 nearest = currentPos;
        //             }
        //         }
        //         if (math.distance(nearest, transform.ValueRO.Position) < ability.range)
        //         {
        //             var forward = Vector3.RotateTowards(transform.ValueRO.Forward(), nearest - transform.ValueRO.Position, math.PI * SystemAPI.Time.DeltaTime, 0);
        //             transform.ValueRW.Rotation = quaternion.LookRotation(forward, Vector3.up);
        //         }
        //     }

        //     physicVel.ValueRW.Linear = math.mul(transform.ValueRO.Rotation, abilityTrigger.ValueRO.ability.velocity);

        //     if (Utils.OverlapFlag((uint)ability.destroyFlag, (uint)AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.ValueRO.Position, abilityTrigger.ValueRO.origin) > ability.range)
        //     {
        //         ecb.DestroyEntity(entity);
        //     }
        // }

        new AbilityTriggerMoveJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel(state.Dependency).Complete();

        new AbilityTriggerJob()
        {
            ecb = ecb,
            grids = grids,
            l2wComponents = l2wComponents,
            colliders = colliders,
            resources = resources,
            deltaTime = SystemAPI.Time.DeltaTime,

        }.ScheduleParallel();

        new AbilityTriggerCollisionJob()
        {
            ecb = ecb,
            resources = resources,
            grids = grids,
            l2wComponents = l2wComponents,
            allColliders = colliders,
            deltaTime = SystemAPI.Time.DeltaTime,
        }.ScheduleParallel(state.Dependency).Complete();
    }
}

[BurstCompile]
public partial struct AbilityTriggerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> grids;
    [ReadOnly] public ComponentLookup<CharacterResourceComponent> resources;
    [ReadOnly] public ComponentLookup<LocalToWorld> l2wComponents;
    [ReadOnly] public ComponentLookup<WorldCircleCollider> colliders;
    public float deltaTime;

    public void Execute(in AbilityTriggerComponent abilityTrigger, ref LocalTransform transform, in Entity entity, [EntityIndexInQuery] int sortKey)
    {
        AbilityComponent ability = abilityTrigger.ability;

        if (abilityTrigger.ability.autoAim)
        {
            NativeList<Entity> validEntities = grids.GetValidEntities(transform.Position, ability.range, Allocator.TempJob);
            float3 nearest = new float3(math.INFINITY, math.INFINITY, math.INFINITY);
            foreach (var current in validEntities)
            {
                if (current == entity) continue;
                if (!resources.HasComponent(current) || !colliders.HasComponent(current)) continue;
                if (!Utils.OverlapFlag(colliders[entity].collideWith, colliders[current].belongTo)) continue;

                float currentDistance = math.distance(l2wComponents[current].Position, l2wComponents[entity].Position);
                float nearestDistance = math.distance(nearest, transform.Position);
                if (currentDistance < nearestDistance)
                {
                    nearest = l2wComponents[current].Position;
                }
            }
            if (math.distance(nearest, transform.Position) < ability.range)
            {
                var forward = Vector3.RotateTowards(transform.Forward(), nearest - transform.Position, math.PI * deltaTime, 0);
                transform.Rotation = quaternion.LookRotation(forward, Vector3.up);
            }
            validEntities.Dispose();
        }

        if (Utils.OverlapFlag((uint)ability.destroyFlag, (uint)AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.Position, abilityTrigger.origin) > ability.range)
        {
            ecb.DestroyEntity(sortKey, entity);
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
    [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> grids;
    [ReadOnly] public ComponentLookup<LocalToWorld> l2wComponents;
    [ReadOnly] public ComponentLookup<WorldCircleCollider> allColliders;
    [ReadOnly] public ComponentLookup<CharacterResourceComponent> resources;
    public float deltaTime;

    public void Execute(in AbilityTriggerComponent abilityTrigger, ref LocalTransform transform, in Entity entity, [EntityIndexInQuery] int sortKey)
    {
        AbilityComponent ability = abilityTrigger.ability;
        var validEntities = grids.GetValidEntities(transform.Position, allColliders[entity].radius, Allocator.TempJob);
        var hits = new NativeList<Entity>(Allocator.TempJob);

        float2 pos = new float2(l2wComponents[entity].Position.x, l2wComponents[entity].Position.z);
        foreach (var validEntity in validEntities)
        {
            if (!resources.HasComponent(validEntity) || !allColliders.HasComponent(validEntity)) continue;
            if (!Utils.OverlapFlag(allColliders[entity].collideWith, allColliders[validEntity].belongTo)) continue;
            float2 targetPos = new float2(l2wComponents[validEntity].Position.x, l2wComponents[validEntity].Position.z);
            if (math.distance(pos, targetPos) < (allColliders[entity].radius + allColliders[validEntity].radius))
            {
                hits.Add(validEntity);
            }
        }

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var resource = resources[hit];
                resource.TakeDamage(ability.damage, ability.elemental);
                ecb.SetComponent<CharacterResourceComponent>(sortKey, hit, resource);
            }

            if (Utils.OverlapFlag((uint)ability.destroyFlag, (uint)AbilityDestroyFlag.OnHit))
            {
                ecb.DestroyEntity(sortKey, entity);
            }
        }
        hits.Dispose();
        validEntities.Dispose();
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