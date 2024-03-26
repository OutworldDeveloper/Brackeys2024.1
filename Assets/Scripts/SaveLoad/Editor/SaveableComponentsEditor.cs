using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveableComponents))]
public sealed class SaveableComponentsEditor : Editor
{

    [SerializeField] private bool _isEditable;

    public override void OnInspectorGUI()
    {
        var componentsListProperty = serializedObject.FindProperty("_saveableComponents");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Add component:");

        if (GUILayout.Button("Edit", GUILayout.Width(35f), GUILayout.Height(15f)))
        {
            _isEditable = !_isEditable;
        }

        EditorGUILayout.EndHorizontal();

        Object objectToAdd = EditorGUILayout.ObjectField(null, typeof(MonoBehaviour), true);

        if (objectToAdd != null)
        {
            Undo.RecordObject(target, "Add Saveable Component");
            (target as SaveableComponents).TryAdd(objectToAdd as MonoBehaviour);
        }

        for (int i = 0; i < componentsListProperty.arraySize; i++)
        {
            EditorGUILayout.Space(4);

            var componentInfoProperty = componentsListProperty.GetArrayElementAtIndex(i);
            var componentProperty = componentInfoProperty.FindPropertyRelative("Component");
            var guidProperty = componentInfoProperty.FindPropertyRelative("Guid");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginDisabledGroup(_isEditable == false);

            EditorGUILayout.PropertyField(componentProperty);

            EditorGUILayout.PropertyField(guidProperty);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            bool wantsRemove = GUILayout.Button("X", GUILayout.Width(15f), GUILayout.Height(15f));

            if (wantsRemove == true)
            {
                string componentName = componentProperty.objectReferenceValue.GetType().Name;

                wantsRemove = EditorUtility.
                    DisplayDialog
                    (
                        $"Remove {componentName}?",
                        $"Are you sure you want to remove {componentName} component from saveables? Guid will be lost.",
                        "Confirm",
                        "Cancel"
                    );
            }

            if (wantsRemove == true)
            {
                Undo.RecordObject(target, "Remove Saveable Component");
                (target as SaveableComponents).RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}
