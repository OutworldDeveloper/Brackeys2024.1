﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(Item), true)]
public sealed class ItemEditor : Editor
{

    private TypeCache.TypeCollection _tagTypes;
    private GenericMenu _tagsMenu;

    private List<Editor> _editors = new List<Editor>();

    private void OnEnable()
    {
        _tagTypes = TypeCache.GetTypesDerivedFrom<ItemTag>();

        _tagsMenu = new GenericMenu();

        foreach (var tagType in _tagTypes)
        {
            _tagsMenu.AddItem(new GUIContent(tagType.Name), false, () =>
            {
                TryAddComponent(tagType);
            });
        }

        CreateEditors();
    }

    private void CreateEditors()
    {
        foreach (var editor in _editors)
        {
            DestroyImmediate(editor);
        }

        _editors.Clear();

        var tagsProperty = serializedObject.FindProperty("_tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            var objectReferenceValue = tagsProperty.GetArrayElementAtIndex(i).objectReferenceValue;
            _editors.Add(CreateEditor(objectReferenceValue));
        }
    }

    private void ClearUnused()
    {
        var tagsProperty = serializedObject.FindProperty("_tags");

        foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(target)))
        {
            bool contains = false;

            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).objectReferenceValue == subAsset)
                {
                    contains = true;
                }
            }

            if (contains == false)
            {
                AssetDatabase.RemoveObjectFromAsset(subAsset);
                DestroyImmediate(subAsset, true);
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(target);
    }

    private void TryAddComponent(Type tagType)
    {
        var tagsProperty = serializedObject.FindProperty("_tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            if (tagsProperty.GetArrayElementAtIndex(i).objectReferenceValue.GetType() == tagType)
                return;
        }

        ItemTag tag = ScriptableObject.CreateInstance(tagType) as ItemTag;
        tag.name = tagType.Name;
        AssetDatabase.AddObjectToAsset(tag, target);

        var index = tagsProperty.arraySize;
        tagsProperty.InsertArrayElementAtIndex(index);
        tagsProperty.GetArrayElementAtIndex(index).objectReferenceValue = tag;
        serializedObject.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(tag);

        CreateEditors();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var tagsProperty = serializedObject.FindProperty("_tags");

        EditorGUI.indentLevel++;

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            GUILayout.Space(10f);

            var tag = tagsProperty.GetArrayElementAtIndex(i).objectReferenceValue;

            GUILayout.BeginVertical(tag.GetType().ToString(), "window");

            GUILayout.Space(5f);

            _editors[i].OnInspectorGUI();

            GUILayout.Space(5f);

            GUILayout.Button("Remove Tag");

            GUILayout.EndVertical();
        }


        GUILayout.Space(10f);

        if (EditorGUILayout.DropdownButton(new GUIContent("Add Tag"), FocusType.Passive))
        {
            _tagsMenu.ShowAsContext();
        }

        //if (GUILayout.Button("Clear unused") == true)
        //    ClearUnused();
    }

}