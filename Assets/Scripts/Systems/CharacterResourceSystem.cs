using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial struct CharacterResourceSystem : ISystem
{
    void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((var resource, var entity) in SystemAPI.Query<RefRO<CharacterResourceComponent>>().WithEntityAccess().WithAll<EnemyComponent>())
        {
            if (resource.ValueRO.hp <= 0)
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}