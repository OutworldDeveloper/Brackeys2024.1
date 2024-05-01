using DG.Tweening;
using UnityEngine;

public sealed class DoorMover : DoorPart
{

    [SerializeField] private Vector3 _originalLocalPosition;
    [SerializeField] private Vector3 _openOffset;

    private void OnValidate()
    {
        _originalLocalPosition = transform.localPosition;
    }

    public override void Set(float t)
    {
        transform.localPosition = Vector3.Lerp(_originalLocalPosition, _originalLocalPosition + _openOffset, t);
    }

    private void OnDrawGizmosSelected()
    {
        if (TryGetComponent(out MeshFilter meshFilter) == false)
            return;

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Gizmos.matrix = transform.parent.localToWorldMatrix;

        Gizmos.DrawMesh(meshFilter.sharedMesh, _originalLocalPosition + _openOffset, transform.localRotation);
    }

}
