using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct AbilityControlSystem : ISystem
{
    void OnCreate(ref SystemState state)
    {
    }

    void OnDestroy(ref SystemState state)
    {
    }

    void OnUpdate(ref SystemState state)
    {
        foreach (var abilityControl in SystemAPI.Query<RefRW<AbilityControlComponent>>())
        {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            if (!abilityControl.ValueRO.currentAbility.Equals(default(AbilityComponent)))
            {
                if (abilityControl.ValueRO.abilityCooldown < 0)
                {
                    var entity = ecb.Instantiate(abilityControl.ValueRO.abilityTriggerPrefab);
                    SystemAPI.SetComponent(entity, new AbilityTriggerComponent()
                    {
                        ability = abilityControl.ValueRO.currentAbility,
                        mask = abilityControl.ValueRO.damageMask,
                        origin = abilityControl.ValueRO.attackTransform.Position,
                    });
                }
            }
        }
    }
}
