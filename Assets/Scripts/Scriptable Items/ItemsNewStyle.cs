using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class ItemD
{
    public readonly string Name;
    public readonly string Description;

    public ItemD(string name, string description)
    {
        Name = name;
        Description = description;
    }

}

public static class Items
{

    private static readonly Dictionary<string, ItemD> _registry = new Dictionary<string, ItemD>(); 

    public static void RegisterItems()
    {
        //_registry.Add(ID.item_rope, new ItemD("Rope", "Just a simple rope"));
        //_registry.Add(ID.item_hook, new ItemD("Hook", "Just a simple hook"));
        //_registry.Add(ID.item_pizza, new ItemD("Pizza", "Just a simple pizza"));
    }

    public static string[] GetIndexes()
    {
        return _registry.Keys.ToArray();
    }

}

public static class ID
{
    public const string item_rope = nameof(item_rope);
    public const string item_hook = nameof(item_hook);
    public const string item_pizza = nameof(item_pizza);

}

public static class RuntimeItemsInitializer
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void RegisterItems()
    {
        Items.RegisterItems();
    }

}

public static class EditorItemsInitializer
{

    [DidReloadScripts]
    public static void OnRecompile()
    {
        Items.RegisterItems();
    }

}

[Serializable]
public class ItemID
{
    [SerializeField] private string _id;

}

[CustomPropertyDrawer(typeof(ItemID))]
public class IngredientDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var amountRect = new Rect(position.x, position.y, 30, position.height);
        var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
        var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        //EditorGUI.PropertyField(position, property.FindPropertyRelative("_id"), GUIContent.none);

        bool wantsChangeID = EditorGUI.DropdownButton(position, new GUIContent(property.FindPropertyRelative("_id").stringValue), FocusType.Passive);

        if (wantsChangeID == true)
        {
            var indexesMenu = new GenericMenu();

            foreach (var index in Items.GetIndexes())
            {
                indexesMenu.AddItem(new GUIContent(index), false, () =>
                {
                    property.FindPropertyRelative("_id").stringValue = index;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            indexesMenu.DropDown(new Rect() 
            { 
                height = position.height,
                width = position.width + 50,
                x = position.x,
                y = position.y
            });
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
