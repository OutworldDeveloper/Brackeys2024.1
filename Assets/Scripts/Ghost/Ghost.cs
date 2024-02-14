using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class Ghost : MonoBehaviour
{

    [SerializeField] private float _respawnTime;
    [SerializeField] private AudioSource _ambientAudioSource;
    [SerializeField] private AudioSource _damageAudioSource;

    private NavMeshAgent _agent;
    private GhostState _state = GhostState.Idle;
    private PlayerCharacter _target;
    private TimeUntil _timeUntilRespawn;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private GhostChaseTargetModifier _currentChaseModifier;

    public GhostState State => _state;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _ambientAudioSource.volume = 0f;
        _damageAudioSource.volume = 0f;
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
        _currentChaseModifier = _target.ApplyModifier(new GhostChaseTargetModifier(this), -1f);
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

    private void UpdateIdle()   
    {
        if (_ambientAudioSource.volume > 0f)
        {
            _ambientAudioSource.volume -= Time.deltaTime;
        }
    }

    private void UpdateChasing()
    {
        _agent.stoppingDistance = 1f;
        _agent.SetDestination(_target.transform.position);

        float targetDistance = Vector3.Distance(_target.transform.position, transform.position);

        if (targetDistance < 2.5f)
        {
            _target.ApplyDamage(1f * Time.deltaTime);
            _damageAudioSource.volume = Mathf.Min(1f, _damageAudioSource.volume + Time.deltaTime * 1f);

            if (targetDistance < 0.5f)
            {
                _target.Kill(DeathType.Psionic);
            }
        }
        else
        {
            _damageAudioSource.volume = Mathf.Max(0f, _damageAudioSource.volume - Time.deltaTime * 0.5f);
        }

        if (_ambientAudioSource.volume < 1f)
        {
            _ambientAudioSource.volume += Time.deltaTime;
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
        _damageAudioSource.volume = 0f;
        _state = GhostState.Idle;
        _agent.ResetPath();
        _agent.Warp(_startPosition);
        transform.rotation = _startRotation;

        if (_currentChaseModifier != null)
            _target.TryRemoveModifier(_currentChaseModifier);
    }

}

public enum GhostState
{
    Idle,
    Chasing,
    Respawning,
}

public sealed class GhostChaseTargetModifier : CharacterModifier
{

    private const float _minSpeedMultiplier = 0.1f;

    private readonly Ghost _ghost;
    private float _currentSpeedMultiplier = 1f;
    private float _currentGhostDistance;

    public GhostChaseTargetModifier(Ghost ghost)
    {
        _ghost = ghost;
    }

    public override void Tick()
    {
        _currentGhostDistance = GetDistanceToGhost();

        if (_currentGhostDistance < 4.5f)
        {
            if (_currentSpeedMultiplier > _minSpeedMultiplier)
                _currentSpeedMultiplier -= Time.deltaTime * 0.15f;
        }
        else
        {
            if (_currentSpeedMultiplier < 1f)
                _currentSpeedMultiplier += Time.deltaTime * 0.5f;
        }
    }

    public override bool CanJump()
    {
        return _currentGhostDistance > 3f;
    }

    public override float GetSpeedMultiplier()
    {
        return _currentSpeedMultiplier;
    }

    private float GetDistanceToGhost()
    {
        return Vector3.Distance(_ghost.transform.position, Character.transform.position);
    }

}
