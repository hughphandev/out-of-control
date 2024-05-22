using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct EnemySpawnerSystem : ISystem
{

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        foreach (var spawner in SystemAPI.Query<RefRW<EnemySpawnerComponent>>())
        {
            if (spawner.ValueRO.timer < 0)
            {
                var playerTrans = SystemAPI.GetComponentRO<LocalTransform>(spawner.ValueRO.player);
                var enemy = state.EntityManager.Instantiate(spawner.ValueRO.enemy);

                var enemyComp = SystemAPI.GetComponentRW<EnemyComponent>(enemy);
                enemyComp.ValueRW.target = spawner.ValueRO.player;

                float distance = UnityEngine.Random.Range(spawner.ValueRO.minDistance, spawner.ValueRO.maxDistance);
                float angle = UnityEngine.Random.Range(0.0f, 360.0f);
                var enemyTrans = SystemAPI.GetComponentRW<LocalTransform>(enemy);
                enemyTrans.ValueRW.Position = playerTrans.ValueRO.Position + math.mul(quaternion.Euler(0, angle, 0), new float3(0.0f, 0.0f, distance));

                spawner.ValueRW.timer = spawner.ValueRO.delay;
            }
            else
            {
                spawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}
