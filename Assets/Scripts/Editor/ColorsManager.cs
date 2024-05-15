using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[FilePath("Tools/Colors", FilePathAttribute.Location.ProjectFolder)]
public class ColorsManager : ScriptableSingleton<ColorsManager>
{

    [SerializeField] private List<ColorPreset> _colorPresets = new List<ColorPreset>();

    public int PresetsCount => _colorPresets.Count;

    public void AddPreset(string presetName, Color color)
    {
        _colorPresets.Add(new ColorPreset(presetName, color));
        Save(true);
    }

    public void RemovePreset(ColorPreset preset) 
    { 
        _colorPresets.Remove(preset);
        Save(true);
    }

    public void SetColor(int index, Color color)
    {
        _colorPresets[index].Color = color;
        Save(true);
    }

    public ColorPreset GetPresetAt(int index)
    {
        return _colorPresets[index];
    }

}
