using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
public partial struct WorldGridSystem : ISystem
{
    [BurstCompile]
    void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new WorldGridComponent()
        {
            grids = new NativeParallelMultiHashMap<int2, Entity>(5, Allocator.Persistent),
        });
    }

    [BurstCompile]
    void OnDestroy(ref SystemState state)
    {
        var grids = SystemAPI.GetSingleton<WorldGridComponent>().grids;
        grids.Dispose();
    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        var grids = SystemAPI.GetSingleton<WorldGridComponent>().grids;
        grids.Clear();

        foreach ((var l2w, Entity entity) in SystemAPI.Query<RefRO<LocalToWorld>>().WithEntityAccess())
        {
            grids.Add(new int2((int)l2w.ValueRO.Position.x, (int)l2w.ValueRO.Position.z), entity);
        }
    }

}

[Serializable]
public struct WorldGridComponent : IComponentData
{
    public NativeParallelMultiHashMap<int2, Entity> grids;
}



