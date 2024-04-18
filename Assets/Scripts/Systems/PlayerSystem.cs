using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class PlayerSystem : SystemBase
{
    private Controls controls = null;

    protected override void OnCreate()
    {
        controls = new Controls();
        controls.Enable();
    }

    protected override void OnDestroy()
    {
        controls = new Controls();
        controls.Disable();
    }

    protected override void OnUpdate()
    {
        foreach (var (mover, transform, velocity) in SystemAPI.Query<RefRW<MoverComponent>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>())
        {
            float2 inputDirection = controls.Character.Move.ReadValue<Vector2>().normalized;

            float3 accelaration = Vector3.zero;
            accelaration.x = inputDirection.x * mover.ValueRO.horizontalAccelaration;
            accelaration.z = inputDirection.y * mover.ValueRO.horizontalAccelaration;

            //TODO: ODE!
            accelaration.x -= velocity.ValueRO.Linear.x * mover.ValueRO.friction;
            accelaration.z -= velocity.ValueRO.Linear.z * mover.ValueRO.friction;

            Cursor.lockState = CursorLockMode.None;
            if (mover.ValueRO.canRotate && !inputDirection.Equals(float2.zero))
            {
                float angle = Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg;
                Debug.Log($"Angle: {inputDirection}");
                transform.ValueRW.Rotation = Quaternion.RotateTowards(transform.ValueRO.Rotation, Quaternion.Euler(0.0f, angle, 0.0f), mover.ValueRO.rotateSpeed * SystemAPI.Time.DeltaTime);
            }

            if (mover.ValueRO.canMove)
            {
                // Vector3 deltaP = (velocity.ValueRO.Linear + accelaration * SystemAPI.Time.DeltaTime * .5f) * SystemAPI.Time.DeltaTime;
                // controller.Move(deltaP);
                velocity.ValueRW.Linear += SystemAPI.Time.DeltaTime * accelaration;
            }
        }

    }
}
