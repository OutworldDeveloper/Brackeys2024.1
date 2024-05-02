using UnityEngine;

[System.Serializable]
public sealed class ColorPreset
{
    public string Name;
    public Color Color;

    public ColorPreset(string name, Color color)
    {
        Name = name;
        Color = color;
    }

}
