using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PlayerSystem : SystemBase
{
    private Controls controls = null;
    private float holdTime;

    protected override void OnCreate()
    {
        controls = new Controls();
        controls.Enable();
        controls.Character.Move.started += MoveStarted;
    }

    protected override void OnDestroy()
    {
        controls = new Controls();
        controls.Enable();
        controls.Character.Move.started += MoveStarted;
    }

    protected override void OnUpdate()
    {
        foreach (var (player, transform, velocity) in SystemAPI.Query<RefRO<CharacterComponent>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>())
        {
            holdTime += SystemAPI.Time.DeltaTime * player.ValueRO.rotationSpeed;
            float2 move = controls.Character.Move.ReadValue<Vector2>().normalized;
            if (move.x != 0 || move.y != 0)
            {
                transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, quaternion.LookRotationSafe(new float3(move.x, 0, move.y), new float3() { y = 1 }), holdTime);
            }

            velocity.ValueRW.Linear = new float3(player.ValueRO.speed * move.x, 0.0f, player.ValueRO.speed * move.y);
        }
    }

    private void MoveStarted(InputAction.CallbackContext context)
    {
        holdTime = 0;
    }
}
