using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerSystem : SystemBase
{
    private Controls controls = null;

    protected override void OnCreate()
    {
        controls = new Controls();
        controls.Enable();
    }

    protected override void OnUpdate()
    {
        foreach (var (player, transform) in SystemAPI.Query<RefRO<CharacterComponent>, RefRW<LocalTransform>>())
        {
            float2 move = controls.Character.Move.ReadValue<Vector2>();
            Debug.Log(move + " - " + player.ValueRO.speed + " - " + SystemAPI.Time.DeltaTime);
            transform.ValueRW.Position.x += player.ValueRO.speed * SystemAPI.Time.DeltaTime * move.x;
            transform.ValueRW.Position.z += player.ValueRO.speed * SystemAPI.Time.DeltaTime * move.y;
        }
    }
}
