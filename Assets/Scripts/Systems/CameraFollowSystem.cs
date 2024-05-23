
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

public partial class CameraFollowSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<PlayerComponent>().ForEach((ref LocalTransform transform) =>
            {
                GameObjectWorld.Instance.cameraFollow.position = transform.Position;
            }).Run();

    }
}