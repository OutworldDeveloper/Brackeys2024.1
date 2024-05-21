using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Sensor : MonoBehaviour
{

    [SerializeField] private float _radius = 13f;
    [SerializeField] private float _detectRadius = 4f;
    [SerializeField] private float _angle = 80f;
    [SerializeField] private float _height = 1f;
    [SerializeField] private LayerMask _checkLayers;
    [SerializeField] private LayerMask _occlusionLayers;
    [SerializeField] private float _checkRate = 0.2f;

    private TimeSince _timeSinceLastCheck;
    private Collider[] _overlaps = new Collider[15];
    private int _overlapsCount;

    private readonly List<GameObject> _targets = new List<GameObject>(15);

    public bool HasTargets => _targets.Count > 0f;
    public GameObject FirstTarget => _targets[0];

    public int FilterTargets<T>(T[] buffer) where T : Component
    {
        int count = 0;
        for (int i = 0; i < _targets.Count; i++)
        {
            GameObject target = _targets[i];

            if (target.transform.root.TryGetComponent(out T component) == false)
                continue;

            buffer[count++] = component;

            if (buffer.Length == count)
                break;
        }

        return count;
    }

    public T GetFirstTarget<T>() where T : Component
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            GameObject target = _targets[i];

            if (target.transform.root.TryGetComponent(out T component) == false)
                continue;

            return component;
        }

        return null;
    }

    public bool HasTarget(GameObject gameObject)
    {
        return _targets.Contains(gameObject);
    }

    private void Update()
    {
        if (_timeSinceLastCheck < _checkRate)
            return;

        _timeSinceLastCheck = TimeSince.Now();
        Scan();
    }

    private void Scan()
    {
        _overlapsCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, _overlaps, _checkLayers);

        _targets.Clear();

        for (int i = 0; i < _overlapsCount; i++)
        {
            GameObject gameObject = _overlaps[i].gameObject;

            if (IsInSight(gameObject) == true)
            {
                _targets.Add(gameObject);
            }
        }
    }

    private bool IsInSight(GameObject target)
    {
        Vector3 origin = transform.position;
        Vector3 destination = target.transform.position;
        Vector3 direction = destination - origin;

        float distance = Vector3.Distance(origin, destination);

        if (direction.y < 0f && direction.y > _height)
            return false;

        direction.y = 0f;
        direction.Normalize();

        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > _angle && distance > _detectRadius)
            return false;

        origin.y += _height / 2;
        destination.y = origin.y;

        if (Physics.Linecast(origin, destination, _occlusionLayers) == true)
            return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(_angle / 2f, Vector3.up) * transform.forward * _radius);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-_angle / 2f, Vector3.up) * transform.forward * _radius);

        Gizmos.color = Color.white;
        for (int i = 0; i < _overlapsCount; i++)
        {
            if (gameObject == null)
                continue; 

            GameObject overlap = _overlaps[i].gameObject;
            Gizmos.DrawSphere(overlap.transform.position, 0.5f);
        }

        Gizmos.color = Color.green;
        foreach (var target in _targets)
        {
            if (gameObject == null)
                continue;

            Gizmos.DrawSphere(target.transform.position, 0.5f);
        }
    }

}
