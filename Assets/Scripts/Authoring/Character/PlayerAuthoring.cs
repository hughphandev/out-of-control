using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : AutoAuthoring<PlayerComponent>
{
}
[Serializable]
public struct PlayerComponent : IComponentData
{
}