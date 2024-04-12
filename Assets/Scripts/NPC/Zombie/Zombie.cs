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

    public enum ThinkState
    {
        NoTarget,
        HasTarget,
        StupidApproach,
        Attack,
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

    private EnumState<Action> _currentAction = new EnumState<Action>();
    private EnumCall<Action> _updateCall;

    private bool _isAttackPointReached;

    private TimeSince _timeSinceLastThink = TimeSince.Never;

    [Persistent] private float _health = 100f;

    private float _attackCooldown = 0f;

    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private bool _isSprinting;

    private EnumState<ThinkState> _thinkState = new EnumState<ThinkState>();
    private EnumCall<ThinkState> _thinkCall;

    public bool IsDead => _health <= 0f;
    public bool HasTarget => _target != null;
    private float TargetDistance => Vector3.Distance(transform.position, _target.transform.position);
    private float TargetAngle => FlatVector.Angle(transform.forward.Flat(), (_target.transform.position - transform.position).normalized.Flat());

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _currentAction.StateStarted.
            AddCallback(Action.None, OnNoneActionStart).
            AddCallback(Action.Attack, OnAttackActionStart).
            AddCallback(Action.Hurt, OnHurtActionStart).
            AddCallback(Action.Roar, OnRoarActionStart);

        _updateCall = _currentAction.AddCall().
            AddCallback(Action.None, OnNoneActionUpdate).
            AddCallback(Action.Attack, OnAttackActionUpdate).
            AddCallback(Action.Hurt, OnHurtActionUpdate).
            AddCallback(Action.Roar, OnRoarActionUpdate);

        _thinkCall = _thinkState.AddCall().
            AddCallback(ThinkState.NoTarget, OnNoTargetThink).
            AddCallback(ThinkState.HasTarget, OnHasTargetThink).
            AddCallback(ThinkState.StupidApproach, OnStupidApproachThink).
            AddCallback(ThinkState.Attack, OnAttackThink);

        _thinkState.StateEnded.
            AddCallback(ThinkState.Attack, OnAttackThinkExit);
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
        _thinkState.Set(ThinkState.HasTarget);
    }

    public void Damage()
    {
        if (IsDead == true)
            return;

        _health -= 15f;

        if (IsDead == false)
        {
            if (Randomize.Chance(1) == true && _currentAction.GetTimeSinceLast(Action.Hurt) > 0.4f)
            {
                _currentAction.Set(Action.Hurt);
                _thinkState.Set(ThinkState.StupidApproach);
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

        if (_currentAction.Current != Action.None)
            return;

        if (_timeSinceLastThink < 0.1f)
            return;

        _timeSinceLastThink = TimeSince.Now();
        _thinkCall.Execute();
    }

    private void Think()
    {
        _thinkCall.Execute();

        return;

        if (HasTarget == false)
            return;

        float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

        float targetAngle = FlatVector.Angle(
            transform.forward.Flat(),
            (_target.transform.position - transform.position).normalized.Flat());

        if (targetDistance < _attackDistance && _currentAction.GetTimeSinceLast(Action.Attack) > _attackCooldown)
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
                _currentAction.Set(Action.Attack);
            return;
        }

        if (targetDistance < 3.5f && Randomize.Chance(35) && _currentAction.GetTimeSinceLast(Action.Attack) > 4f)
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
                _currentAction.Set(Action.Attack);
            return;
        }

        if (targetDistance > 3.0f && 
            Randomize.Chance(35) && 
            ZombieManager.Instance.IsRoarAvaliable && 
            _currentAction.GetTimeSinceLast(Action.Roar) > 6f)
        {
            _currentAction.Set(Action.Roar);
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
            if (_timeSinceLastSprint > 2f || targetDistance < 3.5f)
                _isSprinting = false;
        }
    }

    private void OnNoTargetThink()
    {
        Debug.Log("No target. Nothing to think about really.");
    }

    private void OnHasTargetThink()
    {
        if (Randomize.Chance(10) == true)
        {
            Debug.Log("Starting stupid appraoch");
            _thinkState.Set(ThinkState.StupidApproach);
        }
        else
        {
            Debug.Log("We have target but do nothing");
        }
    }

    private TimeSince _timeSinceLastDirectionChange = TimeSince.Never;

    [SerializeField] private float _toPlayerMltp = 0.3f;
    [SerializeField] private float _forwardMltp = 0.5f;

    private void OnStupidApproachThink()
    {
        TryAttackIfMakesSense();

        if (Randomize.Chance(30) == true)
        {
            _thinkState.Set(ThinkState.Attack);
            return;
        }

        if (TargetDistance < 3.5f && _thinkState.TimeSinceLastChange > 0.4f)
        {
            _thinkState.Set(ThinkState.Attack); // if enough attackers then keep walking slowly
            return;
        }

        if (_timeSinceLastDirectionChange < 0.75f)
            return;

        _timeSinceLastDirectionChange = TimeSince.Now();

        int rays = 16;

        Vector3 bestSpot = transform.position;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < rays; i++)
        {
            Vector3 rayDirection = Quaternion.AngleAxis(i *(360 / 16), Vector3.up) * transform.forward;

            NavMesh.Raycast(transform.position, transform.position + rayDirection * 3f, out NavMeshHit hit, _agent.areaMask);

            Debug.DrawRay(transform.position, rayDirection * hit.distance, hit.hit ? Color.red : Color.green, 0.1f);

            float score = hit.distance / 3f;

            score +=
                (Vector3.Dot(rayDirection, (_target.transform.position - transform.position).normalized) + 1f) / 2
                * _toPlayerMltp;

            score +=
                (Vector3.Dot(rayDirection, transform.forward) + 1f) / 2
                * _forwardMltp;

            score += Randomize.Float(0.0f, 0.4f); // Not sure

            if (score > bestScore)
            {
                bestScore = score;
                bestSpot = transform.position + rayDirection * hit.distance;
            }
        }

        _agent.SetDestination(bestSpot);
    }

    private void OnAttackThink()
    {
        _agent.stoppingDistance = _agent.radius + 0.5f;
        _agent.SetDestination(_target.transform.position);

        if (_isSprinting == false && _timeSinceLastSprint > 14f && TargetDistance > 5f && Randomize.Chance(40))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }

        if (_isSprinting == true)
        {
            if (_timeSinceLastSprint > 2f || TargetDistance < 3.5f)
                _isSprinting = false;
        }

        TryAttackIfMakesSense();
    }

    private void OnAttackThinkExit()
    {
        Notification.ShowDebug("OnAttackThinkExit");
        _isSprinting = false;
    }

    private void TryAttackIfMakesSense()
    {
        if (TargetDistance < _attackDistance)
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
                _currentAction.Set(Action.Attack);
            return;
        }

        if (TargetDistance < 3.5f && Randomize.Chance(35))
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
                _currentAction.Set(Action.Attack);
            return;
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

        _animator.CrossFade("attack", 0.1f);

        _attackCooldown = Randomize.Float(0.0f, 1.5f);
    }

    private void OnAttackActionUpdate()
    {
        RotateTo((_target.transform.position - transform.position).normalized, 2f);

        if (_isAttackPointReached == false && _currentAction.TimeSinceLastChange > 1.1f / 1.5f)
        {
            _isAttackPointReached = true;
            OnAttackPoint();
        }

        if (_currentAction.TimeSinceLastChange > 2.15f / 1.5f)
        {
            if (Randomize.Int(0, 5) == 0)
                _currentAction.Set(Action.Roar);
            else
                _currentAction.Set(Action.None);
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

        _target.ApplyModifier(new AttackedByZombieModifier(), 0.55f);
        _target.ApplyDamage(1f, (_target.transform.position - transform.position).normalized);

        _hitSound.Play(_audioSource);
    }

    private void OnHurtActionStart()
    {
        _animator.Play("hurt");
    }

    private void OnHurtActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange < 1.75f)
            return;

        _currentAction.Set(Action.None);
    }

    private void OnRoarActionStart()
    {
        _animator.Play("roar");
        _roarSound.Play(_audioSource);

        ZombieManager.Instance.NotifyRoar();
    }

    private void OnRoarActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange < 2.6f)
            return;

        _currentAction.Set(Action.None);
    }

    private void RotateTo(Vector3 direction, float speed)
    {
        transform.forward = Vector3.RotateTowards(transform.forward, direction, speed * Time.deltaTime, 0f);
    }

    private bool CanMove()
    {
        return IsDead == false && _currentAction.Current == Action.None;
    }

    private float GetSpeed()
    {
        if (_thinkState == ThinkState.StupidApproach)
            return 0.75f; // костыль

        return _isSprinting ? 5f : _speed;
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

    private AttackCoin _attackCoin = new AttackCoin();

    public bool IsGrabAvaliable => _timeSinceLastGrab > 5f;
    public bool IsRoarAvaliable => _timeSinceLastRoar > 4f;

    public void NotifyGrab()
    {
        _timeSinceLastGrab = TimeSince.Now();
    }

    public void NotifyRoar()
    {
        _timeSinceLastRoar = TimeSince.Now();
    }

    public bool TryTakeAttackCoin(Zombie zombie, float duration)
    {
        if (_attackCoin.IsAvaliable == false)
            return false;

        _attackCoin.DisableFor(duration);
        return true;
    }

    public sealed class AttackCoin
    {

        private TimeUntil _timeUntilReady = new TimeUntil(float.NegativeInfinity);

        public bool IsAvaliable => _timeUntilReady < 0f;

        public void DisableFor(float duration)
        {
            _timeUntilReady = new TimeUntil(Time.time + duration);
        }

    }

}

public sealed class AttackedByZombieModifier : CharacterModifier
{

    public override bool CanJump()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0.45f;
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
