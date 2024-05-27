using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct DelayedDestroySystem : ISystem
{
    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var component in SystemAPI.Query<RefRW<DelayedDestroyComponent>>())
        {
            if (component.ValueRO.delay < 0)
            {
                ecb.DestroyEntity(component.ValueRO.entity);
            }
            else
            {
                component.ValueRW.delay -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}
