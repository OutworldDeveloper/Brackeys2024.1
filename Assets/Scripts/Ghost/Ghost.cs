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
    private GhostState _state = GhostState.Idle;
    private PlayerCharacter _target;
    private TimeUntil _timeUntilRespawn;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public GhostState State => _state;

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
            case GhostState.Idle: UpdateIdle(); break;
            case GhostState.Chasing: UpdateChasing(); break;
            case GhostState.Respawning: UpdateRespawning(); break;
        }
    }

    public void StartChase(PlayerCharacter target)
    {
        _target = target;
        _target.Died += OnTargetDied;
        _state = GhostState.Chasing;
    }

    public void StartRespawning()
    {
        if (_state == GhostState.Respawning)
            return;

        _agent.ResetPath();
        _timeUntilRespawn = new TimeUntil(Time.time + _respawnTime);
        _state = GhostState.Respawning;
    }

    private void UpdateIdle() { }

    private void UpdateChasing()
    {
        _agent.stoppingDistance = 1f;
        _agent.SetDestination(_target.transform.position);

        if (Vector3.Distance(_target.transform.position, transform.position) < 2.5f)
        {
            _target.ApplyDamage(1f * Time.deltaTime);
            //_target.Kill();
        }
    }

    private void OnTargetDied(DeathType deathType)
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
        _state = GhostState.Idle;
        _agent.ResetPath();
        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }

}

public enum GhostState
{
    Idle,
    Chasing,
    Respawning,
}