using System;
using System.Collections;
using System.Collections.Generic;
using AutoAuthoring;
using Unity.Entities;
using UnityEngine;

public class ElementalAuthoring : MonoBehaviour
{
    [LabeledArray(typeof(Elemental))] public GameObject[] prefabs = new GameObject[GameConstant.ElementalTypeCount];

    class Baker : Baker<ElementalAuthoring>
    {
        public override void Bake(ElementalAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<ElementalBufferElement>(entity);
            foreach (var prefab in authoring.prefabs)
            {
                buffer.Add(new ElementalBufferElement()
                {
                    vfxPrefab = GetEntity(prefab, TransformUsageFlags.Renderable),
                });
            }
        }
    }
}

[Serializable]
public struct ElementalBufferElement : IBufferElementData
{
    public Entity vfxPrefab;
}