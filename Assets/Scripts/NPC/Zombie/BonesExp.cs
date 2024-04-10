using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonesExp : MonoBehaviour
{

    [SerializeField] private float _t = 5f;
    [SerializeField] private Transform _target;
    [SerializeField] private float _dot = -0.2f;

    //private Vector3 _currentForward;

    private Vector3 _addedDirection = Vector3.zero;

    private void Start()
    {
        //_currentForward = transform.forward;
    }

    public void Solve()
    {
        //_currentForward = transform.forward;

        Vector3 desiredDirection = (_target.position - transform.position).normalized;

        if (Vector3.Dot(transform.root.forward, desiredDirection) < _dot)
        {
            desiredDirection = transform.forward;
        }

        _addedDirection = Vector3.RotateTowards(transform.forward + _addedDirection, desiredDirection, _t * Time.deltaTime, 0f) - transform.forward;


        transform.forward += _addedDirection;

        //transform.forward = Vector3.RotateTowards(_currentForward, desiredDirection, _t * Time.deltaTime, 0f);
    }

}
