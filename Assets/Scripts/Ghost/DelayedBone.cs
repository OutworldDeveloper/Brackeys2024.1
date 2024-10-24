using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedBone : MonoBehaviour
{

    private float _speed = 10f;

    private Vector3 _offset;
    private Vector3 _currentPosition;
    private Vector3 DesiredPosition => transform.parent.TransformPoint(_offset);

    private void Start()
    {
        _speed += Random.Range(-2f, 2f);

        _offset = transform.localPosition;
        _currentPosition = transform.position;
    }

    private void LateUpdate()
    {
        _currentPosition = Vector3.Lerp(_currentPosition, DesiredPosition, Time.deltaTime * _speed * Random.Range(0.9f, 1.1f));

        transform.position = _currentPosition;
    }

}
