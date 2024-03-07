using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class BoxCheck : MonoBehaviour
{

    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    public bool Check<T>() where T : Component
    {
        Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(_boxCollider.center), _boxCollider.bounds.extents);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<T>() == true)
            {
                return true;
            }
        }

        return false;
    }

}
