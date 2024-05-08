using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct AbilityControlSystem : ISystem
{
    void OnCreate(ref SystemState state)
    {
    }

    void OnDestroy(ref SystemState state)
    {
    }

    void OnUpdate(ref SystemState state)
    {
        foreach ((var abilityControl, var transform) in SystemAPI.Query<RefRW<AbilityControlComponent>, RefRW<LocalTransform>>())
        {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            if (!abilityControl.ValueRO.currentAbility.Equals(default(AbilityComponent)))
            {
                if (abilityControl.ValueRO.timer < 0)
                {
                    var currentAbility = abilityControl.ValueRO.currentAbility;
                    var currentTriggerEntity = ecb.Instantiate(abilityControl.ValueRO.abilityTriggerPrefab);
                    var attackL2W = state.EntityManager.GetComponentData<LocalToWorld>(abilityControl.ValueRO.attackTransform);
                    ecb.SetComponent(currentTriggerEntity, new AbilityTriggerComponent()
                    {
                        entity = currentTriggerEntity,
                        ability = currentAbility,
                        mask = abilityControl.ValueRO.damageMask,
                        origin = attackL2W.Position,
                    });

                    ecb.SetComponent(currentTriggerEntity, new LocalTransform()
                    {
                        Position = attackL2W.Position,
                        Rotation = quaternion.identity,
                        Scale = 1,
                    });

                    if (!currentAbility.velocity.Equals(float3.zero))
                    {
                        ecb.AddComponent(currentTriggerEntity, new PhysicsVelocity()
                        {
                            Linear = math.mul(attackL2W.Rotation, currentAbility.velocity)
                        });
                    }

                    if (currentAbility.destroyFlag.OverlapFlag(AbilityDestroyFlag.OnLifeTimeEnd))
                    {
                        ecb.AddComponent(currentTriggerEntity, new DelayedDestroyComponent()
                        {
                            entity = currentTriggerEntity,
                            delay = currentAbility.lifeTime,
                        });
                    }

                    abilityControl.ValueRW.timer = currentAbility.coolDown;
                }
            }

            abilityControl.ValueRW.timer -= SystemAPI.Time.DeltaTime;
        }
    }
}
