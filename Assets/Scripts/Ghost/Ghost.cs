using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class Ghost : MonoBehaviour
{

    [SerializeField] private float _respawnTime;

    private NavMeshAgent _agent;
    private State _state = State.Idle;
    private PlayerCharacter _target;
    private TimeUntil _timeUntilRespawn;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chasing: UpdateChasing(); break;
            case State.Respawning: UpdateRespawning(); break;
        }
    }

    public void StartChase(PlayerCharacter target)
    {
        _target = target;
        _target.Died += OnTargetDied;
        _state = State.Chasing;
    }

    public void StartRespawning()
    {
        _agent.ResetPath();
        _timeUntilRespawn = new TimeUntil(Time.time + _respawnTime);
        _state = State.Respawning;
    }

    private void UpdateIdle() { }

    private void UpdateChasing()
    {
        _agent.SetDestination(_target.transform.position);

        if (Vector3.Distance(_target.transform.position, transform.position) < 1f)
        {
            _target.Kill();
        }
    }

    private void OnTargetDied()
    {
        StartRespawning();
    }

    private void UpdateRespawning()
    {
        if (_timeUntilRespawn < 0)
        {
            RespawnNow();
        }
    }

    private void RespawnNow()
    {
        _state = State.Idle;
        _agent.ResetPath();
        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }

    private enum State
    {
        Idle,
        Chasing,
        Respawning,
    }

}
