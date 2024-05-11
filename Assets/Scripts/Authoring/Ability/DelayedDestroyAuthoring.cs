using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class DelayedDestroyAuthoring : MonoBehaviour
{
    public float delay;
    class Baker : Baker<DelayedDestroyAuthoring>
    {
        public override void Bake(DelayedDestroyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DelayedDestroyComponent()
            {
                entity = entity,
                delay = authoring.delay,
            });
        }
    }
}

[Serializable]
public struct DelayedDestroyComponent : IComponentData
{
    public Entity entity;
    public float delay;
}