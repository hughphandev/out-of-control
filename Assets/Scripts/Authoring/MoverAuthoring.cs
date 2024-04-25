using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class MoverAuthoring : AutoAuthoring<MoverComponent> { }

[Serializable]
public struct MoverComponent : IComponentData
{
    public bool canMove;
    public bool canRotate;
    public float rotateSpeed;
    public float horizontalAccelaration;
    public float friction;
}