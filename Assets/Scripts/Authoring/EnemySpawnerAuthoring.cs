using System;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    public float minDistance;
    public float maxDistance;
    public float delay;
    public int batch;
    private class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EnemySpawnerComponent()
            {
                enemy = GetEntity(authoring.enemy, TransformUsageFlags.Dynamic),
                player = GetEntity(authoring.player, TransformUsageFlags.WorldSpace),
                minDistance = authoring.minDistance,
                maxDistance = authoring.maxDistance,
                delay = authoring.delay,
                batch = authoring.batch,
            });
        }
    }
}

[Serializable]
public struct EnemySpawnerComponent : IComponentData
{
    public Entity enemy;
    public Entity player;
    public float minDistance;
    public float maxDistance;
    public float delay;
    public float timer;
    public int batch;
}