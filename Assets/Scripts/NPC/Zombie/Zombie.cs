using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{

    // Когда заагрились доступные действия
    // >Порычать грозно
    // Если игрок близко, схватить игрока
    // Create Zombie Manager when needed (MonoBehaviour)

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _grabDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    private NavMeshAgent _agent;
    private PlayerCharacter _target;

    private bool _isAttacking;
    private TimeSince _timeSinceLastAttackStarted;
    private bool _isAttackPointReached;

    [Persistent] private bool _isDead;

    private bool _isGrabbingPlayer;
    private TimeSince _timeSinceLastGrabStarted = TimeSince.Never;

    private TimeSince _timeSinceLastDestinationSet = TimeSince.Never;

    public bool HasTarget => _target != null;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
    }

    private void Start()
    {
        _animator.SetBool("dead", _isDead);
    }

    public void StartChase(PlayerCharacter target)
    {
        _target = target;
    }

    public void Kill()
    {
        _isDead = true;
        _animator.SetBool("dead", true);
    }

    private void Update()
    {
        _agent.speed = CanMove() ? _speed : 0f;

        _animator.SetFloat("velocity", _agent.velocity.magnitude);

        if (HasTarget == true && _isDead == false)
        {
            _agent.stoppingDistance = _agent.radius + 0.2f;

            if (_timeSinceLastDestinationSet > 0.3f)
                _agent.SetDestination(_target.transform.position);

            float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

            float angle = FlatVector.Angle(
                transform.forward.Flat(),
                (_target.transform.position - transform.position).normalized.Flat());

            if (CanGrab() == true && _target.HasModifier<ZombieGrabCooldownModifier>() == false && targetDistance < _grabDistance && angle < 90f)
            {
                _timeSinceLastGrabStarted = TimeSince.Now();
                _isGrabbingPlayer = true;

                Vector3 lookDirection = (transform.position + Vector3.up * 1.8f - _target.Head.position).normalized;

                _target.ApplyModifier(new GrabbedByZombieModifier(this, lookDirection, new TimeUntil(Time.time + 1.2f)), 1.3f); // Remove on zombie death
                _target.ApplyModifier(new ZombieGrabCooldownModifier(), 5f);

                _animator.Play("grab");
            }

            if (CanAttack() == true && targetDistance < _attackDistance)
            {
                _timeSinceLastAttackStarted = TimeSince.Now();
                _isAttacking = true;

                _animator.Play("attack");
            }
        }

        if (_isGrabbingPlayer == true)
        {
            if (_timeSinceLastGrabStarted > 2.5f)
            {
                _isGrabbingPlayer = false;

            }
        }

        if (_isAttacking == true)
        {
            if (_isDead == true)
                _isAttacking = false;

            if (_isAttackPointReached == false && _timeSinceLastAttackStarted > 1.03f)
            {
                _isAttackPointReached = true;
                OnAttackPoint();
            }

            if (_timeSinceLastAttackStarted > 2.15f)
            {
                _isAttacking = false;
                _isAttackPointReached = false;
            }
        }

        // Rotation
        Vector3 desiredFacingDirection = (_isAttacking || _isGrabbingPlayer) ?
            (_target.transform.position - transform.position).normalized :
            _agent.velocity.normalized;

        if (_isDead == false)
        {
            float rotationSpeed = _isAttacking ? 2f : 4f;
            transform.forward = Vector3.RotateTowards(transform.forward, desiredFacingDirection, rotationSpeed * Time.deltaTime, 0f);
        }
    }

    private void OnAttackPoint()
    {
        float angle = FlatVector.Angle(
            transform.forward.Flat(), 
            (_target.transform.position - transform.position).normalized.Flat());

        Notification.ShowDebug($"Angle: {angle}");

        if (angle > 60f)
            return;

        float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

        if (targetDistance > _attackLandDistance)
            return;

        _target.ApplyModifier(new AttackedByZombieModifier(), 0.5f);
        _target.ApplyDamage(1f);
    }

    private bool CanMove()
    {
        return _isDead == false && _isAttacking == false && _isGrabbingPlayer == false;
    }

    private bool CanAttack()
    {
        return false;

        return _isDead == false && _isAttacking == false  && _isGrabbingPlayer == false && _timeSinceLastAttackStarted > 2.6f;
    }

    private bool CanGrab()
    {
        return _isDead == false && _isAttacking == false && _isGrabbingPlayer == false && _timeSinceLastGrabStarted > 2.5f + 4f;
    }

}

public sealed class ZombieManager : MonoBehaviour
{

    private static ZombieManager _instance;

    public static ZombieManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(nameof(ZombieManager)).AddComponent<ZombieManager>();
            }

            return _instance;
        }
    }

    private TimeSince _timeSinceLastGrab = TimeSince.Never;
    
    public bool IsGrabAvaliable => _timeSinceLastGrab > 5f;

    public void NotifyGrab()
    {
        _timeSinceLastGrab = TimeSince.Now();
    }

}

public sealed class AttackedByZombieModifier : CharacterModifier
{

    public override bool CanCrouch()
    {
        return false;
    }

    public override bool CanJump()
    {
        return false;
    }

    public override bool CanInteract()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0.2f;
    }

}

public sealed class GrabbedByZombieModifier : CharacterModifier
{

    private readonly Zombie _zombie;
    private readonly Vector3 _direction;
    private readonly TimeUntil _timeUntilAllowRotation;

    public GrabbedByZombieModifier(Zombie zombie, Vector3 direction, TimeUntil timeUntilAllowRotation)
    {
        _zombie = zombie;
        _direction = direction;
        _timeUntilAllowRotation = timeUntilAllowRotation;
    }

    public override bool CanCrouch()
    {
        return false;
    }

    public override bool CanJump()
    {
        return false;
    }

    public override bool CanInteract()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0.0f;
    }

    public override bool OverrideLookDirection(out Vector3 direction)
    {
        if (_timeUntilAllowRotation < 0)
        {
            direction = default;
            return false;
        }

        direction = _direction;
        return true;
    }

}

public sealed class ZombieGrabCooldownModifier : CharacterModifier { }
