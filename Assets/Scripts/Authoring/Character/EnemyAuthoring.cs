using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public GameObject target;
    public float movementSpeed;

    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyComponent()
            {
                movementSpeed = authoring.movementSpeed,
                target = GetEntity(authoring.target, TransformUsageFlags.WorldSpace),
            });
        }
    }
}

[Serializable]
public struct EnemyComponent : IComponentData
{
    public Entity target;
    public float movementSpeed;
}