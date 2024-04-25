
using System;
using UnityEngine;

public class GameConstant
{
    public static int ElementalTypeCount = Enum.GetNames(typeof(Elemental)).Length;
    public const int MaxRuneCount = 5;
    public const float ProjectileSpacing = 15.0f;
    public const int maxMinionCount = 5;
}