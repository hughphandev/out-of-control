using System;
using Unity.Entities;
using UnityEngine;

public enum HitboxShape
{
    Box,
    Sphere,
}

[Flags]
public enum AbilityDestroyFlag
{
    OnHit = 1 << 0,
    OnAnimationEnd = 1 << 1,
    OnLifeTimeEnd = 1 << 2,
    OnOutOfRange = 1 << 3,
    immediate = 1 << 4,
}

public enum AbilitySpawnLocation
{
    AttackTransform, Target, Self
}

[Flags]
public enum AbilityTag
{
    Melee = 1 << 0,
    Projectile = 1 << 1,
    Elemental = 1 << 2,
    Minion = 1 << 3,
}

public enum AnimationState
{
    None = 0,
    Performing,
    RegisterHitbox,
};

[Flags]
public enum AnimationProperty : int
{
    None = 0,
    GoThroughEnemy = 0b1,
    RootMotion = 0b10,
}

public enum Elemental : int
{
    Normal = 0,
    Fire,
    Wind,
    Lighting,
    Earth,
    Water,
}


[Serializable]

public struct ElementalVFX
{
    public Entity Normal;
    public Entity Fire;
    public Entity Wind;
    public Entity Lightning;
    public Entity Earth;
    public Entity Water;

    public Entity this[Elemental key]
    {
        get => this[(int)key];
        set => this[(int)key] = value;
    }

    public Entity this[int key]
    {
        get
        {
            switch (key)
            {
                case 0:
                    return Normal;
                case 1:
                    return Fire;
                case 2:
                    return Wind;
                case 3:
                    return Lightning;
                case 4:
                    return Earth;
                case 5:
                    return Water;
                default:
                    Debug.LogError($"Index {key} out of bound!");
                    return default;
            }
        }
        set
        {
            switch (key)
            {
                case 0:
                    Normal = value;
                    return;
                case 1:
                    Fire = value;
                    return;
                case 2:
                    Wind = value;
                    return;
                case 3:
                    Lightning = value;
                    return;
                case 4:
                    Earth = value;
                    return;
                case 5:
                    Water = value;
                    return;
                default:
                    Debug.LogError($"Index {key} out of bound!");
                    return;
            }
        }
    }
}