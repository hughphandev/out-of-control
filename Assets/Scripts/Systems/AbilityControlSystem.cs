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
                    var entity = ecb.Instantiate(abilityControl.ValueRO.abilityTriggerPrefab);
                    var attackL2W = state.EntityManager.GetComponentData<LocalToWorld>(abilityControl.ValueRO.attackTransform);
                    ecb.SetComponent(entity, new AbilityTriggerComponent()
                    {
                        ability = abilityControl.ValueRO.currentAbility,
                        mask = abilityControl.ValueRO.damageMask,
                        origin = attackL2W.Position,
                    });

                    ecb.SetComponent(entity, new LocalTransform()
                    {
                        Position = attackL2W.Position,
                        Rotation = quaternion.identity,
                        Scale = 1,
                    });

                    if (!abilityControl.ValueRO.currentAbility.velocity.Equals(float3.zero))
                    {
                        ecb.AddComponent(entity, new PhysicsVelocity()
                        {
                            Linear = math.mul(attackL2W.Rotation, abilityControl.ValueRO.currentAbility.velocity)
                        });
                    }


                    abilityControl.ValueRW.timer = abilityControl.ValueRO.currentAbility.coolDown;
                }
            }

            abilityControl.ValueRW.timer -= SystemAPI.Time.DeltaTime;
        }
    }
}
