using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerSystem : SystemBase
{
    private Controls controls = null;
    private Camera camera = null;

    protected override void OnCreate()
    {
        controls = new Controls();
        controls.Enable();
        camera = Camera.main;
    }

    protected override void OnUpdate()
    {
        foreach (var (player, transform) in SystemAPI.Query<RefRO<CharacterComponent>, RefRW<LocalTransform>>())
        {
            float2 move = controls.Character.Move.ReadValue<Vector2>();
            float2 look = controls.Character.Look.ReadValue<Vector2>();
            const float rotateSpeed = 1.0f;
            Debug.Log(move + " - " + player.ValueRO.speed + " - " + SystemAPI.Time.DeltaTime);
            transform.ValueRW = transform.ValueRW.RotateY(rotateSpeed * SystemAPI.Time.DeltaTime * look.x);
            transform.ValueRW = transform.ValueRO.Translate(transform.ValueRO.TransformDirection(new float3(player.ValueRO.speed * SystemAPI.Time.DeltaTime * move.x, 0.0f, player.ValueRO.speed * SystemAPI.Time.DeltaTime * move.y)));

            camera.transform.position = transform.ValueRO.Position + new float3 { y = 1.0f };
            camera.transform.Rotate(Vector3.right, rotateSpeed * 10.0f * SystemAPI.Time.DeltaTime * look.y);
        }
    }
}
