using System;

public enum HitboxShape
{
    Box,
    Sphere,
}

[Flags]
public enum AbilityDestroy
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