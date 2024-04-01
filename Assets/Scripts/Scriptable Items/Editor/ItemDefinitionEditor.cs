using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDefinition))]
public sealed class ItemDefinitionEditor : Editor
{

    private TypeCache.TypeCollection _componentTypes;
    private GenericMenu _componentsMenu;

    private void OnEnable()
    {
        _componentTypes = TypeCache.GetTypesDerivedFrom<ItemComponent>();

        _componentsMenu = new GenericMenu();

        foreach (var componentType in _componentTypes)
        {
            _componentsMenu.AddItem(new GUIContent(componentType.Name), false, () =>
            {
                TryAddComponent(componentType);
            });
        }
    }

    private void TryAddComponent(Type componentType)
    {
        ItemDefinition itemDefinition = target as ItemDefinition;

        if (itemDefinition.ContainsComponentByDefault(componentType) == true)
            return;

        var defaultComponentsProperty = serializedObject.FindProperty("_defaultComponents");
        var index = defaultComponentsProperty.arraySize;
        defaultComponentsProperty.InsertArrayElementAtIndex(index);
        defaultComponentsProperty.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(componentType, true);
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (EditorGUILayout.DropdownButton(new GUIContent("Add Component"), FocusType.Passive))
        {
            _componentsMenu.ShowAsContext();
        }

        EditorGUI.BeginChangeCheck();

        EditorGUI.indentLevel += 1;

        var defaultComponentsProperty = serializedObject.FindProperty("_defaultComponents");

        for (int i = 0; i < defaultComponentsProperty.arraySize; i++)
        {
            GUILayout.Space(10);

            var componentProperty = defaultComponentsProperty.GetArrayElementAtIndex(i);

            GUILayout.BeginVertical(componentProperty.managedReferenceValue.GetType().Name, "window");

            foreach (var subProperty in GetChildren(componentProperty))
            {
                EditorGUILayout.PropertyField(subProperty, true);
            }

            GUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    public IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
    {
        property = property.Copy();
        var nextElement = property.Copy();
        bool hasNextElement = nextElement.NextVisible(false);
        if (!hasNextElement)
        {
            nextElement = null;
        }

        property.NextVisible(true);
        while (true)
        {
            if ((SerializedProperty.EqualContents(property, nextElement)))
            {
                yield break;
            }

            yield return property;

            bool hasNext = property.NextVisible(false);
            if (!hasNext)
            {
                break;
            }
        }
    }

}
