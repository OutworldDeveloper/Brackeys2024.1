using UnityEditor;
using UnityEngine;

public sealed class ColorsWindow : EditorWindow
{

    [MenuItem("Window/Hallway/Color Presets")]
    public static void Open()
    {
        var window = GetWindow<ColorsWindow>("Color Presets");
        window.Show();
    }

    [SerializeField] private string _selectedName;
    [SerializeField] private Vector2 _scrollPosition;

    private void OnGUI()
    {
        GUILayout.Space(5);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = 0; i < ColorsManager.instance.PresetsCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            ColorPreset preset = ColorsManager.instance.GetPresetAt(i);
            GUILayout.Label(preset.Name, GUILayout.Width(75));
            GUILayout.ExpandWidth(false);

            EditorGUI.BeginChangeCheck();
            var color = EditorGUILayout.ColorField(preset.Color);
            if (EditorGUI.EndChangeCheck() == true)
                ColorsManager.instance.SetColor(i, color);

            if (GUILayout.Button("X", GUILayout.Width(25)) == true)
                ColorsManager.instance.RemovePreset(preset);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        _selectedName = EditorGUILayout.TextField(_selectedName);
        GUILayout.Space(5);

        if (GUILayout.Button("Add") == true)
            ColorsManager.instance.AddPreset(_selectedName, Color.white);
    }

}
