using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class PlayerAuthoring : MonoBehaviour
{
    public float speed = 5.0f;
    public float angularSpeed = 100.0f;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CharacterComponent
            {
                speed = authoring.speed,
                rotationSpeed = authoring.angularSpeed,
            });
        }
    }
}

public struct CharacterComponent : IComponentData
{
    public float speed;
    public float rotationSpeed;
}