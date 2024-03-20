using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    private NavMeshAgent _agent;
    private PlayerCharacter _target;

    private bool _isAttacking;
    private TimeSince _timeSinceLastAttackStarted;
    private bool _isAttackPointReached;

    private bool _isDead;

    public bool HasTarget => _target != null;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
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

        if (HasTarget == true)
        {
            _agent.stoppingDistance = _attackDistance - 0.12f;
            _agent.SetDestination(_target.transform.position);

            float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

            if (_isAttacking == false && targetDistance < _attackDistance && _timeSinceLastAttackStarted > 2.6f)
            {
                _timeSinceLastAttackStarted = TimeSince.Now();
                _isAttacking = true;

                _animator.Play("attack");
            }
        }

        if (_isAttacking == true)
        {
            if (_isDead == true)
                _isAttacking = false;

            if (_isAttackPointReached == false && _timeSinceLastAttackStarted > 1.03f)
            {
                _isAttackPointReached = true;
                AttackPoint();
            }

            if (_timeSinceLastAttackStarted > 2.15f)
            {
                _isAttacking = false;
                _isAttackPointReached = false;
            }
        }

        // Rotation
        Vector3 desiredFacingDirection = _isAttacking ?
            (_target.transform.position - transform.position).normalized :
            _agent.velocity.normalized;

        if (_isDead == false)
        {
            float rotationSpeed = _isAttacking ? 2f : 4f;
            transform.forward = Vector3.RotateTowards(transform.forward, desiredFacingDirection, rotationSpeed * Time.deltaTime, 0f);
        }
    }

    private void AttackPoint()
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
        return _isDead == false && _isAttacking == false;
    }

    private bool CanAttack()
    {
        return _isDead == false && _isAttacking == false && _timeSinceLastAttackStarted > 2.6f;
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
