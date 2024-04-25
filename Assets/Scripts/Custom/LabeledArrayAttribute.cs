using UnityEngine;
using System;

public class LabeledArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public readonly int repeat;
    public LabeledArrayAttribute(string[] names, int repeat = 1) { this.names = names; this.repeat = repeat; }
    public LabeledArrayAttribute(Type enumType, int repeat = 1) { names = Enum.GetNames(enumType); this.repeat = repeat; }
}
