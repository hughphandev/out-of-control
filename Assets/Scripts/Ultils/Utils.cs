using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Physics;
using UnityEngine;

public static class Utils
{
    public static bool OverlapFlag(uint first, uint second)
    {
        return (first & second) != 0;
    }
    public static CollisionFilter LayerMaskToFilter(LayerMask belongMask, LayerMask collideMask)
    {
        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = (uint)collideMask.value,
            CollidesWith = (uint)collideMask.value
        };
        return filter;
    }

    public static void TakeDamage(this ref CharacterResourceComponent resource, float damage, Elemental elemental)
    {
        resource.hp -= damage;
    }
}

class SetComparer<T> : IEqualityComparer<List<T>>
{
    public bool Equals(List<T> x, List<T> y)
    {
        return x.Count == y.Count && x.All(y.Contains);
    }

    public int GetHashCode(List<T> obj)
    {
        int hashcode = 0;
        foreach (T t in obj)
        {
            hashcode ^= t.GetHashCode();
        }
        return hashcode;
    }
}
