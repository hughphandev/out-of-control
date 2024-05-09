using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct AbilityTriggerSystem : ISystem
{
    void OnCreate(ref SystemState state)
    {

    }

    void OnDestroy(ref SystemState state)
    {

    }

    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        foreach ((var abilityTrigger, var transform, var entity) in SystemAPI.Query<RefRO<AbilityTriggerComponent>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            AbilityComponent ability = abilityTrigger.ValueRO.ability;
            if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnOutOfRange) && math.distance(transform.ValueRO.Position, abilityTrigger.ValueRO.origin) > ability.range)
            {
                ecb.DestroyEntity(entity);
            }

            if (ability.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnHit))
            {
                float3 size = ability.hitboxSize;
                var radius = math.max(size.x, math.max(size.y, size.z)) / 2;
                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
                var filter = Utils.LayerMaskToFilter(abilityTrigger.ValueRO.mask, abilityTrigger.ValueRO.mask);
                switch (ability.hitboxShape)
                {
                    case HitboxShape.Box:
                        physicsWorld.BoxCastAll(transform.ValueRO.Position, transform.ValueRO.Rotation, ability.hitboxSize / 2, transform.ValueRO.Forward(), 1, ref hits, filter);
                        break;
                    case HitboxShape.Sphere:
                        physicsWorld.SphereCastAll(transform.ValueRO.Position, radius, float3.zero, 1, ref hits, filter);
                        break;
                    default:
                        Debug.LogError("Hitbox Shape Unhandled!");
                        break;
                }
                if (hits.Length > 0)
                {
                    ecb.DestroyEntity(entity);
                    foreach (var hit in hits)
                    {
                        if (SystemAPI.HasComponent<CharacterResourceComponent>(hit.Entity))
                        {
                            var resource = SystemAPI.GetComponent<CharacterResourceComponent>(hit.Entity);
                            resource.TakeDamage(ability.damage, ability.elemental);
                            ecb.SetComponent<CharacterResourceComponent>(hit.Entity, resource);
                        }
                    }
                }
            }
        }
    }
}
