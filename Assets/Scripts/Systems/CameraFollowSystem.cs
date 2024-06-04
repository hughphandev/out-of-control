
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public partial class CameraFollowSystem : SystemBase
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
        {
            GameObjectWorld.Instance.cameraFollow.position = transform.ValueRO.Position;
        }
    }
}