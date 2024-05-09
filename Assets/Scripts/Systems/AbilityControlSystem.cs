using System.Collections;
using System.Collections.Generic;
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

    void OnUpdate(ref SystemState state)
    {
        foreach ((var abilityControl, var transform, var entity) in SystemAPI.Query<RefRW<AbilityControlComponent>, RefRW<LocalTransform>>().WithAll<AbilityRuntimeBufferElement>().WithEntityAccess())
        {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var abilities = SystemAPI.GetBuffer<AbilityRuntimeBufferElement>(entity);
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Equals(default)) continue;
                if (abilities[i].timer < 0)
                {
                    var currentTriggerEntity = ecb.Instantiate(abilityControl.ValueRO.abilityTriggerPrefab);
                    var attackL2W = state.EntityManager.GetComponentData<LocalToWorld>(abilityControl.ValueRO.attackTransform);
                    ecb.SetComponent(currentTriggerEntity, new AbilityTriggerComponent()
                    {
                        ability = abilities[i].value,
                        mask = abilityControl.ValueRO.damageMask,
                        origin = attackL2W.Position,
                    });

                    ecb.SetComponent(currentTriggerEntity, new LocalTransform()
                    {
                        Position = attackL2W.Position,
                        Rotation = quaternion.identity,
                        Scale = 1,
                    });

                    if (!abilities[i].value.velocity.Equals(float3.zero))
                    {
                        ecb.AddComponent(currentTriggerEntity, new PhysicsVelocity()
                        {
                            Linear = math.mul(attackL2W.Rotation, abilities[i].value.velocity)
                        });
                    }

                    if (abilities[i].value.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnLifeTimeEnd))
                    {
                        ecb.AddComponent(currentTriggerEntity, new DelayedDestroyComponent()
                        {
                            entity = currentTriggerEntity,
                            delay = abilities[i].value.lifeTime,
                        });
                    }

                    abilities[i] = new AbilityRuntimeBufferElement(abilities[i])
                    {
                        timer = abilities[i].value.coolDown,
                    };
                }
                else
                {
                    abilities[i] = new AbilityRuntimeBufferElement(abilities[i])
                    {
                        timer = abilities[i].timer - SystemAPI.Time.DeltaTime,
                    };
                }

            }
        }
    }
}
