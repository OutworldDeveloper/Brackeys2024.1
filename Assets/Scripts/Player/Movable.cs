using UnityEngine;

public sealed class Movable : MonoBehaviour
{
    [field: SerializeField] public Vector3 PlaceCheckExtents { get; private set; }
    [field: SerializeField] public Vector3 PlaceCheckOrigin { get; private set; }

    [SerializeField] private Collider[] _colliders;

    [ContextMenu(nameof(FindCollidersInChildren))]
    private void FindCollidersInChildren()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    public void OnPickedUp()
    {
        foreach (var collider in _colliders)
        {
            collider.enabled = false;
        }
    }

    public void OnPlaced()
    {
        foreach (var collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(PlaceCheckOrigin, PlaceCheckExtents);
    }

}
