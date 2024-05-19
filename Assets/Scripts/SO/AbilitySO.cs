
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Data/Ability", order = 0)]
public class AbilitySO : ScriptableObject
{
    public AbilityComponent value;
    [LabeledArray(typeof(Elemental))] public GameObject[] elementalsPrefabs;

    private void OnValidate()
    {

        if (elementalsPrefabs.Length != GameConstant.ElementalTypeCount) Array.Resize(ref elementalsPrefabs, GameConstant.ElementalTypeCount);
    }
}