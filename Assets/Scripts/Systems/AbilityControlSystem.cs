using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct AbilityControlSystem : ISystem
{
    void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        foreach ((var abilityControl, var abilitiesBuffer, var transform) in SystemAPI.Query<RefRW<AbilityControlComponent>, DynamicBuffer<AbilityRuntimeBufferElement>, RefRW<LocalTransform>>())
        {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var abilities = abilitiesBuffer;
            var filter = Utils.LayerMaskToFilter(abilityControl.ValueRO.layer, abilityControl.ValueRO.damageMask);
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].value.projectileCount == 0) continue;
                if (abilities[i].timer < 0)
                {
                    int projectileCount = abilities[i].value.projectileCount;
                    for (int j = 0; j < projectileCount; ++j)
                    {
                        var currentTriggerEntity = ecb.Instantiate(abilityControl.ValueRO.abilityTriggerPrefab);
                        var attackL2W = state.EntityManager.GetComponentData<LocalToWorld>(abilityControl.ValueRO.attackTransform);
                        var vfx = ecb.Instantiate(abilities[i].elementalsPrefabs[abilities[i].value.elemental]);
                        ecb.AddComponent(vfx, new Parent() { Value = currentTriggerEntity });
                        ecb.AppendToBuffer<LinkedEntityGroup>(currentTriggerEntity, new LinkedEntityGroup() { Value = vfx });

                        ecb.SetComponent(currentTriggerEntity, new AbilityTriggerComponent()
                        {
                            ability = abilities[i].value,
                            damageMask = abilityControl.ValueRO.damageMask,
                            origin = attackL2W.Position,
                        });
                        float3 position;
                        quaternion rot;
                        switch (abilities[i].value.spawnLocation)
                        {
                            case AbilitySpawnLocation.Self:
                                position = transform.ValueRO.Position;
                                rot = math.mul(transform.ValueRO.Rotation, quaternion.RotateY((j - (float)(projectileCount - 1) / 2) * (math.PI / 10.0f)));
                                break;
                            case AbilitySpawnLocation.Target:
                                position = transform.ValueRO.Position;
                                rot = math.mul(transform.ValueRO.Rotation, quaternion.RotateY((j - (float)(projectileCount - 1) / 2) * (math.PI / 10.0f)));
                                break;
                            case AbilitySpawnLocation.AttackTransform:
                                position = attackL2W.Position;
                                rot = math.mul(attackL2W.Rotation, quaternion.RotateY((j - (float)(projectileCount - 1) / 2) * (math.PI / 10.0f)));
                                break;
                            default:
                                position = default;
                                rot = default;
                                Debug.LogError("Invalid value");
                                break;
                        }
                        ecb.SetComponent(currentTriggerEntity, new LocalTransform()
                        {
                            Position = position,
                            Rotation = rot,
                            Scale = 1,
                        });

                        if (Utils.OverlapFlag((uint)abilities[i].value.destroyFlag, (uint)AbilityDestroyFlag.OnLifeTimeEnd))
                        {
                            ecb.AddComponent(currentTriggerEntity, new DelayedDestroyComponent()
                            {
                                entity = currentTriggerEntity,
                                delay = abilities[i].value.lifeTime,
                            });
                        }

                    }
                    abilities.ElementAt(i).timer = abilities[i].value.coolDown;
                }
                else
                {
                    abilities.ElementAt(i).timer -= SystemAPI.Time.DeltaTime;
                }
            }
        }
    }
}
