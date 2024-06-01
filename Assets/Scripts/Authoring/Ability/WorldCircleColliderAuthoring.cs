using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class WorldCircleColliderAuthoring : MonoBehaviour
{
    public LayerMask belongTo;
    public LayerMask collideWith;
    public float radius;

    private class Baker : Baker<WorldCircleColliderAuthoring>
    {
        public override void Bake(WorldCircleColliderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new WorldCircleCollider()
            {
                belongTo = authoring.belongTo.value,
                collideWith = authoring.collideWith.value,
                radius = authoring.radius,
            });
        }
    }
}

[Serializable]
public struct WorldCircleCollider : IComponentData
{
    public int belongTo;
    public int collideWith;
    public float radius;
}