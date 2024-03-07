using System;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    [SerializeField] public List<ItemTag> Tags;

    public bool HasTag(ItemTag tag)
    {
        return Tags.Contains(tag);
    }

    public void DestroyItem()
    {
        Destroy(gameObject);
    }

}
