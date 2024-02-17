using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimation : MonoBehaviour
{

    private Vector3 _originalLocalPosition;

    private void Start()
    {
        _originalLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        transform.localPosition = new Vector3()
        {
            x = _originalLocalPosition.x + Mathf.Sin(Time.time * 1f) * 0.5f,
            y = _originalLocalPosition.y + Mathf.Sin(Time.time * 2f) * 0.5f,
            z = _originalLocalPosition.z,
        };
    }

}
