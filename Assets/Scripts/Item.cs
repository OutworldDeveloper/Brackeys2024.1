using System;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Vector3 FaceDirection { get; private set; } = new Vector3(0f, 0f, 0f);
    [field: SerializeField] public Vector3 InspectOffset { get; private set; }
    [field: SerializeField] public float Distance { get; private set; } = 0.3f;
    [field: SerializeField] public bool IsItem { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field:SerializeField] public StackType StackType { get; private set; }

    [SerializeField] private ItemTag[] _tags;
    [SerializeField] private Collider[] _colliders;
    [SerializeField] private MeshRenderer[] _visuals;

    private int _collisionDisabled;
    private int _visualsDisabled;

    public void DisableCollision()
    {
        _collisionDisabled++;
        UpdateCollisionState();
    }

    public void EnableCollision()
    {
        _collisionDisabled--;
        UpdateCollisionState();
    }

    public void DisableVisuals()
    {
        _visualsDisabled++;
        UpdateVisualsState();
    }

    public void EnableVisuals()
    {
        _visualsDisabled--;
        UpdateVisualsState();
    }

    public bool HasTag(ItemTag tag)
    {
        for (int i = 0; i < _tags.Length; i++)
        {
            if (_tags[i] == tag)
                return true;
        }

        return false;
    }

    public void Kill()
    {
        // Check if we're inside inventory
        Destroy(gameObject);
    }

    private void UpdateCollisionState()
    {
        bool state = _collisionDisabled <= 0;

        foreach (var collider in _colliders)
        {
            collider.enabled = state;
        }
    }

    private void UpdateVisualsState()
    {
        bool state = _visualsDisabled <= 0;

        foreach (var renderer in _visuals)
        {
            renderer.enabled = state;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(InspectOffset, 0.01f);
        Gizmos.DrawLine(InspectOffset,
            Quaternion.AngleAxis(FaceDirection.x, Vector3.right) *
            Quaternion.AngleAxis(FaceDirection.y, Vector3.up) *
            Quaternion.AngleAxis(FaceDirection.z, Vector3.forward) *
            Vector3.forward * Distance);
    }

}
