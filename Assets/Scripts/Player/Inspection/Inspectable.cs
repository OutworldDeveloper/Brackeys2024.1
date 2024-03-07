using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Inspectable : MonoBehaviour
{
    [field: SerializeField] public Vector3 FaceDirection { get; private set; } = new Vector3(0f, 0f, 0f);
    [field: SerializeField] public Vector3 InspectOffset { get; private set; }
    [field: SerializeField] public float Distance { get; private set; } = 0.3f;

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
