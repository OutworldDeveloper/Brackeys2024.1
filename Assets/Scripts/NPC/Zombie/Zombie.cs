using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{

    // Когда заагрились доступные действия
    // >Порычать грозно
    // Если игрок близко, схватить игрока
    // Если не приседает игрок, тогда никак
    // Медленнол идёт, а потом как побежит резко и удар
    // Create Zombie Manager when needed (MonoBehaviour)

    public enum Action
    {
        None,
        Attack,
        Roar,
        Hurt,
        HurtBadly,
        Afk,
    }

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _grabDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _roarSound;

    private NavMeshAgent _agent;
    private PlayerCharacter _target;

    private EnumState<Action> _actionManager = new EnumState<Action>();
    private EnumCall<Action> _updateCall;

    private bool _isAttackPointReached;

    private TimeSince _timeSinceLastThink = TimeSince.Never;

    [Persistent] private float _health = 100f;

    private float _attackCooldown = 0f;

    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private bool _isSprinting;

    public bool IsDead => _health <= 0f;
    public bool HasTarget => _target != null;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _actionManager.StateStarted.
            AddCallback(Action.None, OnNoneActionStart).
            AddCallback(Action.Attack, OnAttackActionStart).
            AddCallback(Action.Hurt, OnHurtActionStart).
            AddCallback(Action.Roar, OnRoarActionStart);

        _updateCall = _actionManager.AddCall().
            AddCallback(Action.None, OnNoneActionUpdate).
            AddCallback(Action.Attack, OnAttackActionUpdate).
            AddCallback(Action.Hurt, OnHurtActionUpdate).
            AddCallback(Action.Roar, OnRoarActionUpdate);
    }

    private void Start()
    {
        if (IsDead == true)
        {
            _animator.Play("dead");
            DisableColliders();
        }
    }

    public void StartChase(PlayerCharacter target)
    {
        _target = target;
    }

    public void Damage()
    {
        if (IsDead == true)
            return;

        _health -= 15f;

        if (IsDead == false)
        {
            if (Randomize.Chance(1) == true && _actionManager.GetTimeSinceLast(Action.Hurt) > 0.4f)
            {
                _actionManager.Set(Action.Hurt);
            }
        }
        else
        {
            DisableColliders();
            _animator.CrossFade("dying", 0.2f);
        }
    }

    private void Update()
    {
        if (IsDead == true)
            return;

        _updateCall.Execute();

        _agent.speed = CanMove() ? GetSpeed() : 0f;

        if (_actionManager.Current != Action.None)
            return;

        if (HasTarget == true)
        {
            _agent.stoppingDistance = _agent.radius + 0.5f;
            _agent.SetDestination(_target.transform.position);
        }

        if (_timeSinceLastThink < 0.1f)
            return;

        _timeSinceLastThink = TimeSince.Now();
        Think();
    }

    private void Think()
    {
        if (HasTarget == false)
            return;

        float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

        float targetAngle = FlatVector.Angle(
            transform.forward.Flat(),
            (_target.transform.position - transform.position).normalized.Flat());

        if (targetDistance < _attackDistance && _actionManager.GetTimeSinceLast(Action.Attack) > _attackCooldown)
        {
            _actionManager.Set(Action.Attack);
            return;
        }

        if (targetDistance < 3.5f && Randomize.Chance(35) && _actionManager.GetTimeSinceLast(Action.Attack) > 4f)
        {
            _actionManager.Set(Action.Attack);
            return;
        }

        if (targetDistance > 3.0f && 
            Randomize.Chance(35) && 
            ZombieManager.Instance.IsRoarAvaliable && 
            _actionManager.GetTimeSinceLast(Action.Roar) > 6f)
        {
            _actionManager.Set(Action.Roar);
            return;
        }

        if (_isSprinting == false && _timeSinceLastSprint > 14f && targetDistance > 5f && Randomize.Chance(40))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }

        if (_isSprinting == true)
        {
            if (_timeSinceLastSprint > 2f)
                _isSprinting = false;

            if (targetDistance < _attackDistance)
            {
                _isSprinting = false;
                _actionManager.Set(Action.Attack);
            }
        }
    }

    private void OnNoneActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
    }

    private void OnNoneActionUpdate()
    {
        RotateTo(_agent.velocity, 4f);

        _animator.SetFloat("velocity", _agent.velocity.magnitude);
    }

    private void OnAttackActionStart()
    {
        _isAttackPointReached = false;

        _animator.Play("attack");

        _attackCooldown = Randomize.Float(0.0f, 1.5f);
    }

    private void OnAttackActionUpdate()
    {
        RotateTo((_target.transform.position - transform.position).normalized, 2f);

        if (_isAttackPointReached == false && _actionManager.TimeSinceLastChange > 1.1f / 1.5f)
        {
            _isAttackPointReached = true;
            OnAttackPoint();
        }

        if (_actionManager.TimeSinceLastChange > 2.15f / 1.5f)
        {
            if (Randomize.Int(0, 5) == 0)
                _actionManager.Set(Action.Roar);
            else
                _actionManager.Set(Action.None);
        }
    }

    private void OnAttackPoint()
    {
        float angle = FlatVector.Angle(
            transform.forward.Flat(),
            (_target.transform.position - transform.position).normalized.Flat());

        Notification.ShowDebug($"Angle: {angle}");

        if (angle > 70f)
            return;

        float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

        if (targetDistance > _attackLandDistance)
            return;

        _target.ApplyModifier(new AttackedByZombieModifier(), 0.5f);
        _target.ApplyDamage(1f);

        _hitSound.Play(_audioSource);
    }

    private void OnHurtActionStart()
    {
        _animator.Play("hurt");
    }

    private void OnHurtActionUpdate()
    {
        if (_actionManager.TimeSinceLastChange < 1.75f)
            return;

        _actionManager.Set(Action.None);
    }

    private void OnRoarActionStart()
    {
        _animator.Play("roar");
        _roarSound.Play(_audioSource);

        ZombieManager.Instance.NotifyRoar();
    }

    private void OnRoarActionUpdate()
    {
        if (_actionManager.TimeSinceLastChange < 2.6f)
            return;

        _actionManager.Set(Action.None);
    }

    private void RotateTo(Vector3 direction, float speed)
    {
        transform.forward = Vector3.RotateTowards(transform.forward, direction, speed * Time.deltaTime, 0f);
    }

    private bool CanMove()
    {
        return IsDead == false && _actionManager.Current == Action.None;
    }

    private float GetSpeed()
    {
        return _isSprinting ? 5f : _speed;
    }

    private float GetTargetDistance()
    {
        return Vector3.Distance(transform.position, _target.transform.position);
    }

    private void DisableColliders()
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
            _agent.enabled = false;
        }
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
    private TimeSince _timeSinceLastRoar = TimeSince.Never;

    public bool IsGrabAvaliable => _timeSinceLastGrab > 5f;
    public bool IsRoarAvaliable => _timeSinceLastRoar > 4f;

    public void NotifyGrab()
    {
        _timeSinceLastGrab = TimeSince.Now();
    }

    public bool TryTakeAttackCoin(Zombie zombie)
    {
        return false;
    }

    public void NotifyRoar()
    {
        _timeSinceLastRoar = TimeSince.Now();
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
        return 0.1f;
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
