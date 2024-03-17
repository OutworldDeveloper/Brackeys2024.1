using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInteraction))]
public sealed class PlayerCharacter : Pawn
{

    public event Action<DeathType> Died;
    public event Action Respawned;
    public event Action Damaged;

    [SerializeField] private Transform _head;
    [SerializeField] private PlayerInteraction _interactor;
    [SerializeField] private Grip _grip;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private FloatParameter _mouseSensitivity;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _maxHealth = 5f;
    [SerializeField] private bool _allowJumping;
    [SerializeField] private bool _allowCrouching;
    [SerializeField] private float _crouchedCameraHeight = -0.75f;
    [SerializeField] private AnimationCurve _crouchAnimation;
    [SerializeField] private AnimationCurve _uncrouchAnimation;
    [SerializeField] private float _crouchAnimationDuration = 0.45f;
    [SerializeField] private LayerMask _uncrouchLayerMask;
    [SerializeField] private float _crouchedControllerSize = 1.25f;

    [SerializeField] private InspectionPawn _inspectionPawn;

    [SerializeField] private WeaponHolder _weaponHolder;

    [SerializeField] private float _aimingSpeed = 0.5f;

    private CharacterController _controller;
    private Vector3 _velocityXZ;
    private float _velocityY;
    private TimeSince _timeSinceLastDeath = TimeSince.Never;
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private readonly List<CharacterModifier> _modifiers = new List<CharacterModifier>();
    private TimeSince _timeSinceLastDamage = TimeSince.Never;
    private PlayerInput _currentInput;

    private float _defaultCameraHeight;
    private bool _isCrouching;
    private TimeSince _timeSinceLastPostureChange = TimeSince.Never;

    private int _lastStairsTouchFrame;
    private bool _isTouchingStairs;
    private readonly string _stairsTag = "Stairs";

    private float _currentCameraHeight;

    private bool _isAiming;

    public PlayerInteraction Interactor => _interactor;
    public Inventory Inventory => _inventory;
    public Grip Grip => _grip;
    public bool IsDead { get; private set; }
    public float RespawnTime => _respawnTime;
    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }
    public Vector3 HorizontalVelocity => _velocityXZ;
    public bool IsGrounded => _controller.isGrounded;
    public bool IsCrouching => _isCrouching;

    public void Inspect(Item target, bool noAnimation = false)
    {
        _inspectionPawn.SetTarget(target, noAnimation);
        Player.Possess(_inspectionPawn);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.moveDirection.y) < 0.01f)
        {
            if (hit.gameObject.TryGetComponent<Rigidbody>(out var rb) == true)
            {
                rb.AddForce(_velocityXZ * 25f);
            }
        }

        if (hit.moveDirection.y > 0.01f && _velocityY > 0f)
            _velocityY = 0f;

        if (Mathf.Abs(hit.moveDirection.y) < 0.01f && hit.gameObject.CompareTag(_stairsTag) == true)
        {
            _lastStairsTouchFrame = Time.frameCount;
            _isTouchingStairs = true;
            _controller.stepOffset = 1.5f;
        }
    }

    private void LateUpdate()
    {
        if (_isTouchingStairs == true && Time.frameCount > _lastStairsTouchFrame)
        {
            _controller.stepOffset = 0.3f;
            _isTouchingStairs = false;
        }
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _defaultCameraHeight = _head.localPosition.y;

        Health = _maxHealth;

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        ApplyModifier(new SpawnBlockModifier(), 0.4f);
    }

    public override void InputTick()
    {
        _currentInput = GatherInput();

        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
            Inspect(_inventory.Items[0], true);

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
            Inspect(_inventory.Items[1], true);

        if (Input.GetKeyDown(KeyCode.Alpha3) == true)
            Inspect(_inventory.Items[2], true);
    }

    private void Update()
    {
        // Modifiers
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            var modifier = _modifiers[i];

            if (modifier.IsInfinite == false && modifier.TimeUntilExpires < 0)
            {
                _modifiers.RemoveAt(i);
            }
            else
            {
                modifier.Tick();
            }
        }

        // Crouching
        if (_isCrouching == false)
        {
            if (_currentInput.WantsCrouch == true && CanCrouch() == true)
            {
                Crouch();
            }
        }
        else
        {
            var shouldUncrouch = _currentInput.WantsCrouch == false || CanCrouch() == false;

            if (shouldUncrouch == true && CanUncrouch())
            {
                Stand();
            }
        }

        // Aiming
        if (_isAiming == false)
        {
            if (_currentInput.WantsAim == true && CanAim() == true)
            {
                _isAiming = true;
            }
        }
        else
        {
            if (_currentInput.WantsAim == false || CanAim() == false)
            {
                _isAiming = false;
            }
        }

        // Update camera rotation
        if (_timeSinceLastPostureChange < _crouchAnimationDuration)
        {
            var targetHeight = _isCrouching ? _crouchedCameraHeight : _defaultCameraHeight;
            var t = _timeSinceLastPostureChange / _crouchAnimationDuration;
            var animationCurve = _isCrouching ? _crouchAnimation : _uncrouchAnimation;
            t = animationCurve.Evaluate(t);
            _head.localPosition = new Vector3()
            {
                x = _head.localPosition.x,
                y = Mathf.Lerp(_head.localPosition.y, targetHeight, t),
                z = _head.localPosition.z
            };
        }

        if (IsDead == true && _timeSinceLastDeath > _respawnTime)
        {
            Respawn();
        }

        if (IsDead == false && _timeSinceLastDamage > 10f)
        {
            Health = Mathf.Min(Health + Time.deltaTime, _maxHealth);
        }

        UpdateRotation(_currentInput);
        UpdateMovement(_currentInput);

        if (CanInteract() == true && _currentInput.WantsInteract == true)
        {
            _interactor.TryPerform(_currentInput.InteractionIndex);
        }

        // test Smooth camera Y
        _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, _head.transform.position.y, 15f * Time.deltaTime);
  
        _currentInput.Clear();
    }

    public override void OnUnpossessed()
    {
        base.OnUnpossessed();
        _velocityXZ = Vector3.zero;
    }

    public void Warp(Vector3 position)
    {
        transform.position = position;
        Physics.SyncTransforms();
    }

    public void Kill(DeathType type)
    {
        _velocityXZ = Vector3.zero;
        IsDead = true;
        _timeSinceLastDeath = new TimeSince(Time.time);
        Died?.Invoke(type);
        GetComponent<Animator>().SetBool("dead", true);
        _modifiers.Clear();
    }

    public void Respawn()
    {
        GetComponent<Animator>().SetBool("dead", false);

        _velocityXZ = Vector3.zero;
        _velocityY = 0f;

        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;
        Physics.SyncTransforms();

        _head.localRotation = Quaternion.identity;
        Health = _maxHealth;

        ApplyModifier(new SpawnBlockModifier(), 0.4f);

        IsDead = false;
        Respawned?.Invoke();
    }

    public T ApplyModifier<T>(T modifier, float duration) where T : CharacterModifier
    {
        modifier.Init(this, duration);
        _modifiers.Add(modifier);
        return modifier;
    }

    public void TryRemoveModifier(CharacterModifier modifier) 
    {
        _modifiers.Remove(modifier);
    }

    public void ApplyDamage(float damage)
    {
        if (IsDead == true)
            return;

        _timeSinceLastDamage = new TimeSince(Time.time);
        Health = Mathf.Max(0f, Health - damage);
        Damaged?.Invoke();

        if (Health <= 0f)
        {
            Kill(DeathType.Psionic);
        }
    }

    private PlayerInput GatherInput()
    {
        var playerInput = new PlayerInput();

        playerInput.MouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity.Value;
        playerInput.MouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity.Value;

        playerInput.Direction = new FlatVector()
        {
            x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0,
            z = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0
        }.normalized;

        playerInput.WantsJump = Input.GetKeyDown(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.F) == true)
            playerInput.InteractionIndex = 0;
        else if (Input.GetKeyDown(KeyCode.E) == true)
            playerInput.InteractionIndex = 1;
        else if (Input.GetKeyDown(KeyCode.B) == true)
            playerInput.InteractionIndex = 2;
        else
            playerInput.InteractionIndex = -1;

        playerInput.WantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        playerInput.WantsAim = Input.GetKey(KeyCode.Mouse1);

        return playerInput;
    }

    private void UpdateRotation(PlayerInput input)
    {
        if (CanRotateHead() == false)
            return;

        var yRotation = transform.eulerAngles.y + input.MouseX;
        var xRotation = _head.localEulerAngles.x - input.MouseY;
        xRotation = ClampAngle(xRotation, -70f, 70f);

        transform.eulerAngles = new Vector3(0f, yRotation, 0f);
        _head.localEulerAngles = new Vector3(xRotation, 0f, 0f);

        float ClampAngle(float angle, float min, float max)
        {
            float start = (min + max) * 0.5f - 180;
            float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
            return Mathf.Clamp(angle, min + floor, max + floor);
        }
    }

    private void UpdateMovement(PlayerInput input)
    {
        Vector3 desiredVelocity = CanWalk() ?
            transform.TransformDirection(input.Direction) * GetSpeed() :
            Vector3.zero;

        _velocityXZ = Vector3.MoveTowards(_velocityXZ, desiredVelocity, 25f * Time.deltaTime);

        if (_controller.isGrounded == true)
        {
            _velocityY = (input.WantsJump && CanJump()) ? _jumpForce : -9.8f;
        }
        else
        {
            _velocityY -= 9.8f * Time.deltaTime;
        }

        Vector3 finalMove = new Vector3()
        {
            x = _velocityXZ.x,
            y = _velocityY,
            z = _velocityXZ.z,
        };

        finalMove *= Time.deltaTime;

        _controller.Move(finalMove);
    }

    private void Crouch()
    {
        _isCrouching = true;
        _timeSinceLastPostureChange = new TimeSince(Time.time);

        // temp
        _controller.height = _crouchedControllerSize;
        _controller.center = new Vector3(_controller.center.x, _controller.height / 2f, _controller.center.z);
    }

    private void Stand()
    {
        _isCrouching = false;
        _timeSinceLastPostureChange = new TimeSince(Time.time);

        // temp
        _controller.height = 2f;
        _controller.center = new Vector3(_controller.center.x, _controller.height / 2f, _controller.center.z);
    }

    private bool CanRotateHead()
    {
        if (IsDead == true)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanRotateCamera() == false)
                return false;
        }

        return true;
    }

    private bool CanInteract()
    {
        if (IsDead == true)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanInteract() == false)
                return false;
        }

        return true;
    }

    private bool CanJump()
    {
        if (IsDead == true)
            return false;

        if (_isCrouching == true)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanJump() == false)
                return false;
        }

        return _allowJumping;
    }

    private bool CanWalk()
    {
        return IsDead == false;
    }

    private float GetSpeed()
    {
        var baseSpeed = _isAiming ? _aimingSpeed : _speed;

        var multipler = 1f;

        foreach (var modifier in _modifiers)
        {
            var modifierMultipler = modifier.GetSpeedMultiplier();
            multipler = Mathf.Min(multipler, modifierMultipler);
        }

        multipler = Mathf.Max(0f, multipler);

        var crouchMultipler = _isCrouching ? 0.4f : 1f;

        return baseSpeed * multipler * crouchMultipler;
    }

    public bool CanCrouch()
    {
        if (_allowCrouching == false)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanCrouch() == false)
                return false;
        }

        return true && _timeSinceLastPostureChange > _crouchAnimationDuration && _controller.isGrounded == true && IsDead == false;
    }

    public bool CanUncrouch()
    {
        return 
            _timeSinceLastPostureChange > _crouchAnimationDuration && 
            Physics.CheckCapsule(
                transform.position + Vector3.up * _controller.radius,
                transform.position + Vector3.up * 2f - Vector3.up * _controller.radius,
                _controller.radius, _uncrouchLayerMask) == false;
    }

    public bool CanAim()
    {
        return IsDead == false && _controller.isGrounded == true;
    }

    public override Vector3 GetCameraPosition() => new Vector3(_head.transform.position.x, _currentCameraHeight, _head.transform.position.z);
    public override Quaternion GetCameraRotation() => _head.rotation;
    public override bool OverrideCameraFOV => _isAiming;
    public override float GetCameraFOV() => 50f;

    private struct PlayerInput
    {
        public float MouseX;
        public float MouseY;
        public FlatVector Direction;
        public bool WantsJump;
        public int InteractionIndex;
        public bool WantsCrouch;
        public bool WantsAim;

        public bool WantsInteract => InteractionIndex != -1;

        public void Clear()
        {
            MouseX = 0;
            MouseY = 0;
            Direction = FlatVector.zero;
            WantsJump = false;
            InteractionIndex = -1;
            WantsCrouch = false;
            WantsAim = false;
        }

    }

}

public enum DeathType
{
    Physical,
    Psionic
}

public abstract class CharacterModifier
{
    public PlayerCharacter Character { get; private set; }
    public TimeUntil TimeUntilExpires { get; private set; }
    public bool IsInfinite { get; private set; }

    public void Init(PlayerCharacter character, float duration)
    {
        Character = character;
        TimeUntilExpires = new TimeUntil(Time.time + duration);
        IsInfinite = duration < 0f;
    }

    public virtual float GetSpeedMultiplier() => 1f;
    public virtual bool CanInteract() => true;
    public virtual bool CanJump() => true;
    public virtual bool CanCrouch() => true;
    public virtual bool CanRotateCamera() => true;
    public virtual void Tick() { }

}

public sealed class SpawnBlockModifier : CharacterModifier
{

    public override bool CanInteract()
    {
        return true;
    }

    public override bool CanJump()
    {
        return false;
    }

    public override bool CanCrouch()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0f;
    }

}
