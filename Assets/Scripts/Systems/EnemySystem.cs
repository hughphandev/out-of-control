using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial struct EnemySystem : ISystem
{
    void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        foreach ((var localTransform, var enemy, var buffer, var velocity, var entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<EnemyComponent>, DynamicBuffer<AbilityRuntimeBufferElement>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            var abilities = buffer;
            if (abilities.Length > 0)
            {
                var playerTrans = SystemAPI.GetComponent<LocalTransform>(enemy.ValueRO.target);
                float range = float.MaxValue;
                foreach (var ability in abilities)
                {
                    if (range > ability.value.range) range = ability.value.range;
                }

                var dir = math.normalize(playerTrans.Position - localTransform.ValueRO.Position);
                localTransform.ValueRW.Rotation = quaternion.LookRotation(dir, Vector3.up);
                if (math.distance(playerTrans.Position, localTransform.ValueRO.Position) > range)
                {
                    velocity.ValueRW.Linear = dir * enemy.ValueRO.movementSpeed;
                    SystemAPI.SetComponentEnabled<AbilityControlComponent>(entity, false);
                }
                else
                {
                    velocity.ValueRW.Linear = float3.zero;
                    SystemAPI.SetComponentEnabled<AbilityControlComponent>(entity, true);
                }
            }
        }
    }
}
