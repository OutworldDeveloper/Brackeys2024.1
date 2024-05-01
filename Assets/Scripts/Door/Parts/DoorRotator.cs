using UnityEngine;

public sealed class DoorRotator : DoorPart
{

    [SerializeField] private RotationType _rotationType;
    [SerializeField] private float _openedAngle;

    public override void Set(float t)
    {
        float angle = Mathf.Lerp(0f, _openedAngle, t);
        transform.localEulerAngles = GetEuler(angle);
    }

    private Vector3 GetEuler(float angle)
    {
        return
            _rotationType == RotationType.X ?
                new Vector3(angle, 0f, 0f) :
                _rotationType == RotationType.Y ?
                        new Vector3(0f, angle, 0f) :
                        new Vector3(0f, 0f, angle);
    }

    private void OnDrawGizmosSelected()
    {
        if (TryGetComponent(out MeshFilter meshFilter) == false)
            return;

        Vector3 localEuler = GetEuler(_openedAngle);

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Gizmos.matrix = transform.parent.localToWorldMatrix;

        Gizmos.DrawMesh(meshFilter.sharedMesh, transform.localPosition, Quaternion.Euler(localEuler));
    }

    private enum RotationType
    {
        X,
        Y,
        Z,
    }

}
