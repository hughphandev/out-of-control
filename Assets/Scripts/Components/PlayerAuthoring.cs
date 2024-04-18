using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;

public class PlayerAuthoring : MonoBehaviour
{
    public MoverComponent mover = new MoverComponent
    {
        canMove = true,
        canRotate = true,
    };

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.mover);
        }
    }
}

[Serializable]
public struct MoverComponent : IComponentData
{
    public bool canMove;
    public bool canRotate;
    public float rotateSpeed;
    public float horizontalAccelaration;
    public float friction;
}