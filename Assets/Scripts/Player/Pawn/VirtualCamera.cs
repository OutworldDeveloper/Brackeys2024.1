using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCamera : MonoBehaviour
{
    [field: SerializeField, Range(1f, 180f)] public float FieldOfView { get; set; } = 75f;

    public CameraState State => new CameraState(transform.position, transform.rotation, FieldOfView);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, FieldOfView, 1000f, 0.2f, 1.77f);
    }

}

public readonly struct CameraState
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly float FieldOfView;

    public CameraState(Vector3 position, Quaternion rotation, float fieldOfView)
    {
        Position = position;
        Rotation = rotation;
        FieldOfView = fieldOfView;
    }

    public static CameraState Lerp(CameraState a, CameraState b, float t)
    {
        Vector3 position = Vector3.Lerp(a.Position, b.Position, t);
        Quaternion rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t);
        float fov = Mathf.Lerp(a.FieldOfView, b.FieldOfView, t);
        return new CameraState(position, rotation, fov);
    }

}
