using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{

    public enum Action
    {
        None,
        Attack,
        Roar,
        Hurt,
        HurtBadly,
        Afk,
        Jump,
        PostJumpDelay,
    }

    public enum ThinkState
    {
        NoTarget,
        InvestigateSound,
        Follow, // Slowly following player (if too far, sprint)
        StupidApproach, // When close enough, start walking around randomly
        Engage, // Rush for attack
    }

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _grabDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _roarSound;

    [SerializeField] private Transform _lookTarget;

    [SerializeField] private Collider _playerBlocker;

    [SerializeField] private Sensor _sensor;
    [SerializeField] private SoundsSensor _soundsSensor;

    [SerializeField] private float _toPlayerMltp = 0.3f;
    [SerializeField] private float _forwardMltp = 0.5f;

    [SerializeField] private MultiAimConstraint _headTargetingConstraint;
    [SerializeField] private MultiAimConstraint _bodyTargetingConstraint;

    [SerializeField] private AnimationCurve _jumpCurve;

    private NavMeshAgent _agent;
    private PlayerCharacter _target;

    private EnumState<Action> _currentAction = new EnumState<Action>();
    private EnumCall<Action> _updateCall;

    private bool _isAttackPointReached;

    private TimeSince _timeSinceLastThink = TimeSince.Never;

    [Persistent] private float _health = 100f;

    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private bool _isSprinting;

    private TimeSince _timeSinceLastDirectionChange = TimeSince.Never;

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

        foreach (var hitbox in GetComponentsInChildren<Hitbox>())
        {
            hitbox.Damaged += OnHitboxDamaged;
        }

        _currentAction.StateStarted.
            AddCallback(Action.None, OnNoneActionStart).
            AddCallback(Action.Attack, OnAttackActionStart).
            AddCallback(Action.Hurt, OnHurtActionStart).
            AddCallback(Action.Roar, OnRoarActionStart).
            AddCallback(Action.Jump, OnJumpActionStart);

        _updateCall = _currentAction.AddCall().
            AddCallback(Action.None, OnNoneActionUpdate).
            AddCallback(Action.Attack, OnAttackActionUpdate).
            AddCallback(Action.Hurt, OnHurtActionUpdate).
            AddCallback(Action.Roar, OnRoarActionUpdate).
            AddCallback(Action.Jump, OnJumpActionUpdate).
            AddCallback(Action.PostJumpDelay, OnPostJumpDelayActionUpdate);

        _thinkCall = _thinkState.AddCall().
            AddCallback(ThinkState.NoTarget, OnNoTargetThink).
            AddCallback(ThinkState.InvestigateSound, OnInvestigateSoundThink).
            AddCallback(ThinkState.Follow, OnFollowThink).
            AddCallback(ThinkState.StupidApproach, OnStupidApproachThink).
            AddCallback(ThinkState.Engage, OnEngageThink);

        _thinkState.StateEnded.
            AddCallback(ThinkState.Engage, OnEngageThinkExit);

        _soundsSensor.Perceived += OnSoundPerceived;
    }

    private void Start()
    {
        if (IsDead == true)
        {
            _animator.Play("dead");
            SetDead();
        }
    }

    public void SetTarget(PlayerCharacter target)
    {
        _target = target;
        _thinkState.Set(ThinkState.Follow);
    }

    private void OnHitboxDamaged(Hitbox hitbox, float damage)
    {
        Notification.ShowDebug($"Hit {hitbox.HitboxType}");

        float damageMultiplier = 
            hitbox.HitboxType == HitboxType.Head ? 2f : hitbox.HitboxType == HitboxType.Body ? 1f : 0.5f;

        ApplyDamage(damage * damageMultiplier);
    }

    public void ApplyDamage(float damage)
    {
        if (IsDead == true)
            return;

        _health -= damage;

        if (IsDead == false)
        {
            if (Randomize.Chance(1) == true && _currentAction.GetTimeSinceLast(Action.Hurt) > 0.4f)
            {
                _currentAction.Set(Action.Hurt);
            }
        }
        else
        {
            SetDead();
            _animator.CrossFade("dying", 0.2f);
        }
    }

    private void OnSoundPerceived(SoundEvent soundEvent)
    {
        if (_thinkState != ThinkState.NoTarget)
            return;

        _thinkState.Set(ThinkState.InvestigateSound);
    }

    private void Update()
    {
        float targetWeight = _target == null ? 0f : IsDead == true ? 0f : _isSprinting == true ? 0f : 1f;
        _headTargetingConstraint.weight = Mathf.Lerp(_headTargetingConstraint.weight, targetWeight, 4f * Time.deltaTime);
        _bodyTargetingConstraint.weight = Mathf.Lerp(_bodyTargetingConstraint.weight, targetWeight * 0.45f, 4f * Time.deltaTime);

        if (IsDead == true)
            return;

        //
        Vector3 targetLookPosition = _target != null ? 
            _target.transform.position + Vector3.up * 1.75f : 
            transform.position + Vector3.up * 1.75f + Vector3.forward;

        _lookTarget.transform.position = Vector3.Lerp(_lookTarget.transform.position, targetLookPosition, 3f * Time.deltaTime);
        //

        _updateCall.Execute();

        _agent.speed = CanMove() ? GetSpeed() : 0f;

        if (_currentAction.Current != Action.None)
            return;

        if (_timeSinceLastThink < 0.1f)
            return;

        _timeSinceLastThink = TimeSince.Now();
        _thinkCall.Execute();
    }

    private void OnNoTargetThink()
    {
        TryFindTargetAndStartChase();
    }

    private void OnInvestigateSoundThink()
    {
        if (TryFindTargetAndStartChase() == true)
            return;

        if (Vector3.Distance(transform.position, _soundsSensor.LastEvent.Position) > 1f)
        {
            _agent.stoppingDistance = 0f;
            _agent.SetDestination(_soundsSensor.LastEvent.Position);
        }
        else
        {
            _thinkState.Set(ThinkState.NoTarget);
        }
    }

    private void OnFollowThink()
    {
        if (TryAttackIfMakesSense() == true)
            return;

        if (TryEngage() == true)
            return;

        if (TargetDistance < 4.0f)
        {
            _thinkState.Set(ThinkState.StupidApproach);
            return;
        }

        _agent.stoppingDistance = 1f;
        _agent.SetDestination(_target.transform.position);
    }

    private void OnStupidApproachThink()
    {
        if (TryAttackIfMakesSense() == true)
            return;

        if (TryEngage() == true)
            return;

        if (TargetDistance > 5.5f)
        {
            _thinkState.Set(ThinkState.Follow);
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
            Vector3 rayDirection = Quaternion.AngleAxis(i * (360 / 16), Vector3.up) * transform.forward;

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

    private void OnEngageThink()
    {
        _agent.stoppingDistance = _agent.radius + 0.5f;
        _agent.SetDestination(_target.transform.position);

        if (_isSprinting == false && _timeSinceLastSprint > 14f && TargetDistance > 5f && Randomize.Chance(40))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }

        // Костыль
        if (_isSprinting == false && TargetDistance > 8.5f && Randomize.Chance(15))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }
        //

        if (_isSprinting == true)
        {
            if (_timeSinceLastSprint > 2f || TargetDistance < 3.5f)
                _isSprinting = false;
        }

        // Should it stay?
        if (_thinkState.TimeSinceLastChange > 8f)
        {
            _thinkState.Set(ThinkState.StupidApproach);
        }

        Try.Any(TryAttackIfMakesSense, TryJump);
    }

    private void OnEngageThinkExit()
    {
        _isSprinting = false;
    }

    private bool TryEngage()
    {
        if (Randomize.Chance(25) == true && ZombieManager.Instance.TryTakeEngageCoin(this, 5f) == true)
        {
            _thinkState.Set(ThinkState.Engage);
            return true;
        }

        if (TargetDistance < 3.5f &&
            _thinkState.TimeSinceLastChange > 0.4f &&
            ZombieManager.Instance.TryTakeEngageCoin(this, 5f) == true)
        {
            _thinkState.Set(ThinkState.Engage);
            return true;
        }

        return false;
    }

    private bool TryAttackIfMakesSense()
    {
        if (TargetDistance < _attackDistance)
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
            {
                _currentAction.Set(Action.Attack);
                return true;
            }

            return false;
        }

        if (TargetDistance < 3.5f && Randomize.Chance(35))
        {
            if (ZombieManager.Instance.TryTakeAttackCoin(this, 1.5f))
            {
                _currentAction.Set(Action.Attack);
                return true;
            }

            return false;
        }

        return false;
    }

    private Vector3 _jumpPosition;

    private bool TryJump()
    {
        if (Randomize.Chance(20) == false)
            return false;

        if (ZombieManager.Instance.TryTakeJumpCoin(this) == false)
            return false;

        int rays = 16;

        Vector3 bestSpot = transform.position;
        float bestScore = float.NegativeInfinity;
        float distance = 0f;

        for (int i = 0; i < rays; i++)
        {
            Vector3 rayDirection = Quaternion.AngleAxis(i * (360 / 16), Vector3.up) * transform.forward;

            NavMesh.Raycast(transform.position, transform.position + rayDirection * 3f, out NavMeshHit hit, _agent.areaMask);

            Debug.DrawRay(transform.position, rayDirection * hit.distance, hit.hit ? Color.red : Color.green, 1f);

            float score = hit.distance / 3f;

            score +=
                (Vector3.Dot(rayDirection, (_target.transform.position - transform.position).normalized) + 1f) / 2
                * _toPlayerMltp;

            score += Randomize.Float(0.0f, 0.4f); // Not sure

            if (score > bestScore)
            {
                bestScore = score;
                bestSpot = transform.position + rayDirection * hit.distance;
                distance = hit.distance;
            }
        }

        if (bestScore < 0f)
            return false;

        if (distance < 2.5f)
            return false;

        _jumpPosition = bestSpot;
        _currentAction.Set(Action.Jump);
        return true;
    }

    private bool TryFindTargetAndStartChase()
    {
        if (_sensor.HasTargets == false)
            return false;

        PlayerCharacter target = _sensor.GetFirstTarget<PlayerCharacter>();

        if (target == null)
            return false;

        SetTarget(target);
        return true;
    }

    private void OnNoneActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
    }

    private void OnNoneActionUpdate()
    {
        RotateTo(_agent.velocity, 5f);

        _animator.SetFloat("velocity", _agent.velocity.magnitude);

        if (Vector3.Angle(transform.forward, _agent.velocity) > 40f)
            _agent.speed = 0f;
    }

    private void OnAttackActionStart()
    {
        _isAttackPointReached = false;
        _animator.CrossFade("attack", 0.1f);
    }

    private void OnAttackActionUpdate()
    {
        RotateTo((_target.transform.position - transform.position).normalized, 1.6f);

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

        ZombieManager.Instance.NotifyRoar(this);
    }

    private void OnRoarActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange < 2.6f)
            return;

        _currentAction.Set(Action.None);
    }

    private Vector3 _jumpStartPosition;
    private bool _almostLanded;

    private void OnJumpActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
        _animator.CrossFade("jump_init", 0.1f, 0);

        _jumpStartPosition = transform.position;

        _almostLanded = false;
    }

    private void OnJumpActionUpdate()
    {
        RotateTo(_jumpPosition - transform.position, 5f);

        const float prepDuration = 0.95f;
        float jumpDuration = Vector3.Distance(_jumpStartPosition, _jumpPosition) * 0.175f;

        if (_currentAction.TimeSinceLastChange > prepDuration + jumpDuration)
        {
            _currentAction.Set(Action.PostJumpDelay);
            return;
        }

        if (_currentAction.TimeSinceLastChange < prepDuration)
            return;

        if (_almostLanded == false && _currentAction.TimeSinceLastChange > prepDuration + jumpDuration - 0.3f)
        {
            _almostLanded = true;
            _animator.CrossFade("jump_land", 0.1f, 0);
        }

        float t = (_currentAction.TimeSinceLastChange - prepDuration) / jumpDuration;
        t = Mathf.SmoothStep(0f, 1f, t);
        //t = _jumpCurve.Evaluate(t);
        Vector3 position = Vector3.Lerp(_jumpStartPosition, _jumpPosition, t);
        _agent.Move(position - transform.position);
    }

    private void OnPostJumpDelayActionUpdate()
    {
        const float duration = 0.8f;

        if (_currentAction.TimeSinceLastChange < duration)
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
        if (_thinkState == ThinkState.StupidApproach || 
            _thinkState == ThinkState.InvestigateSound || 
            _thinkState == ThinkState.Follow)
            return 0.75f; // костыль

        return _isSprinting ? 5f : _speed;
    }

    private void SetDead()
    {
        _agent.enabled = false;
        _playerBlocker.enabled = false;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{_thinkState.Current}");
#endif
    }

}

public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(nameof(T)).AddComponent<T>();
            }

            return _instance;
        }
    }

}

public sealed class ZombieManager : LazySingleton<ZombieManager>
{

    private AttackCoin _engageCoin = new AttackCoin();
    private AttackCoin _attackCoin = new AttackCoin();
    private AttackCoin _jumpCoin = new AttackCoin();

    public void NotifyRoar(Zombie zombie) { }

    public bool TryTakeAttackCoin(Zombie zombie, float duration)
    {
        return TryTakeCoin(_attackCoin, duration);
    }

    public bool TryTakeEngageCoin(Zombie zombie, float duration)
    {
        return TryTakeCoin(_engageCoin, duration);
    }

    public bool TryTakeJumpCoin(Zombie zombie)
    {
        return TryTakeCoin(_jumpCoin, 6f);
    }

    private bool TryTakeCoin(AttackCoin coin, float duration)
    {
        if (coin.IsAvaliable == false)
            return false;

        coin.DisableFor(duration);
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

public static class Try
{
    public static bool This(params Func<bool>[] stack)
    {
        for (int i = 0; i < stack.Length; i++)
        {
            if (stack[i].Invoke() == true)
                return true;
        }

        return false;
    }

    public static bool Any(Func<bool> a, Func<bool> b)
    {
        if (a.Invoke() == true)
            return true;

        return b.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c)
    {
        if (Any(a, b) == true)
            return true;

        return c.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c, Func<bool> d)
    {
        if (Any(a, b, c) == true)
            return true;

        return d.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c, Func<bool> d, Func<bool> e)
    {
        if (Any(a, b, c, d) == true)
            return true;

        return e.Invoke();
    }

}
