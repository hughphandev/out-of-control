using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public static class Utils
{
    public static bool OverlapFlag(uint first, uint second)
    {
        return (first & second) != 0;
    }

    public static bool OverlapFlag(LayerMask first, LayerMask second)
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

    public static NativeList<Entity> GetValidEntities(this NativeParallelMultiHashMap<int2, Entity> grids, float3 pos3D, float range, Allocator allocator = Allocator.Temp)
    {
        var entities = new NativeList<Entity>(allocator);
        int2 position = new int2((int)pos3D.x, (int)pos3D.z);

        int2 min = new int2(position.x - (int)math.ceil(range), position.y - (int)math.ceil(range));
        int2 max = new int2(position.x + (int)math.ceil(range), position.y + (int)math.ceil(range));

        for (int x = min.x; x <= max.x; ++x)
        {
            for (int y = min.y; y <= max.y; ++y)
            {
                var current = new int2(x, y);
                if (grids.ContainsKey(current))
                {
                    var en = grids.GetValuesForKey(current);
                    while (en.MoveNext())
                    {
                        entities.Add(en.Current);
                    }
                }
            }
        }

        return entities;
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
