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
    [SerializeField] private float _aimingFieldOfView = 50f;
    [SerializeField] private float _fieldOfView = 70f;

    [SerializeField] private Animator _armsAnimator;

    [SerializeField] private Equipment _equipment;

    private CharacterController _controller;
    private Vector3 _velocityXZ;
    private float _velocityY;
    private TimeSince _timeSinceLastDeath = TimeSince.Never;

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

    private float _currentFOV = 70f;

    private float _currentRecoilY;
    private float _targetRecoilY;
    private float _currentRecoilX;
    private float _targetRecoilX;
    private TimeSince _timeSinceLastShoot;

    private float _cameraTargetRotX;
    private float _cameraTargetRotY;

    private EnumState<WeaponState> _weaponState;
    private TimeSince _timeSinceLastWeaponStateChange = TimeSince.Never;

    public PlayerInteraction Interactor => _interactor;
    public Inventory Inventory => _inventory;
    public Grip Grip => _grip;
    public bool IsDead { get; private set; }
    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }
    public Vector3 HorizontalVelocity => _velocityXZ;
    public bool IsGrounded => _controller.isGrounded;
    public bool IsCrouching => _isCrouching;

    public void Inspect(Inspectable target, bool noAnimation = false)
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

        if (Mathf.Abs(hit.moveDirection.y) < 0.01f && hit.gameObject.CompareTag(_stairsTag) == true && _isCrouching == false)
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

        _weaponState = new EnumState<WeaponState>(OnWeaponStateChanged);

        _equipment.Initialize();

        _equipment.WeaponSlot.Changed += ItemSlot =>
        {
            if (_weaponState.Current != WeaponState.NoWeapon)
                _weaponState.Set(WeaponState.Unequipping);
        };
    }

    private void Start()
    {
        _defaultCameraHeight = _head.localPosition.y;

        Health = _maxHealth;

        ApplyModifier(new SpawnBlockModifier(), 0.4f);
    }

    public override void InputTick()
    {
        _currentInput = GatherInput();

        if (Input.GetKeyDown(KeyCode.R) == true)
        {
            if (_equipment.WeaponSlot.IsEmpty == false && 
                _equipment.WeaponSlot.Stack.Components.Has<LoadedAmmoComponent>(out var ammoComponent))
            {
                ammoComponent.Value += 5;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) == true)
        {
            if (CanShoot() == true &&
                _equipment.WeaponSlot.Stack.Item is WeaponItem weapon && 
                _timeSinceLastShoot > weapon.Cooldown &&
                _equipment.WeaponSlot.Stack.Components.Has<LoadedAmmoComponent>(out var ammoComponent) && ammoComponent.Value > 0)
            {
                weapon.Shoot(null, _head.transform.position, _head.transform.forward);
                _weaponHolder.ActiveWeapon.OnAttack(_head.transform.position, _head.transform.forward);
                _timeSinceLastShoot = TimeSince.Now();
                _targetRecoilY += UnityEngine.Random.Range(weapon.RecoilVerticalMin, weapon.RecoilVerticalMax);
                //_targetRecoilY += UnityEngine.Random.Range(25f, 35f);
                bool recoilRight = UnityEngine.Random.Range(0, 2) == 0;
                Notification.ShowDebug(recoilRight ? "Right" : "Left");
                _targetRecoilX += UnityEngine.Random.Range(weapon.RecoilHorizontalMin, weapon.RecoilHorizontalMax) * (recoilRight ? 1 : -1);

                //_armsAnimator.Play("shotgun_shoot", 0);
                //_armsAnimator.SetTrigger("shoot");
                _armsAnimator.CrossFadeInFixedTime($"shoot{_weaponHolder.ActiveWeapon.AnimationSet}", 0.02f, 0);

                ammoComponent.Value--;
            }
        }

        if (Application.isEditor == true)
        {
            if (Input.GetKeyDown(KeyCode.F1))
                Time.timeScale = 0.1f;

            if (Input.GetKeyDown(KeyCode.F2))
                Time.timeScale = 0.5f;

            if (Input.GetKeyDown(KeyCode.F3))
                Time.timeScale = 0.75f;

            if (Input.GetKeyDown(KeyCode.F4))
                Time.timeScale = 1f;
        }
    }

    private void OnWeaponStateChanged(WeaponState newState)
    {
        switch (newState)
        {
            case WeaponState.NoWeapon:
                _weaponHolder.RemoveWeapon();
                _armsAnimator.SetInteger("current_weapon", 0);
                break;
            case WeaponState.Equipping:
                _weaponHolder.Equip((_equipment.WeaponSlot.Stack.Item as WeaponItem).WeaponModel);
                _armsAnimator.SetInteger("current_weapon", _weaponHolder.ActiveWeapon.AnimationSet);
                break;
            case WeaponState.Ready:
                _armsAnimator.SetInteger("current_weapon", _weaponHolder.ActiveWeapon.AnimationSet);
                break;
            case WeaponState.Unequipping:
                _armsAnimator.SetInteger("current_weapon", 0);
                break;
        }

        _timeSinceLastWeaponStateChange = TimeSince.Now();
    }

    private void Update()
    {
        // Weapon Equipment
        switch (_weaponState.Current)
        {
            case WeaponState.NoWeapon:
                {
                    bool shouldEquip = _equipment.WeaponSlot.IsEmpty == false;
                    if (shouldEquip == true)
                        _weaponState.Set(WeaponState.Equipping);
                }
                break;

            case WeaponState.Equipping:
                {
                    if (_timeSinceLastWeaponStateChange > 0.8f)
                        _weaponState.Set(WeaponState.Ready);
                }
                break;

            case WeaponState.Ready:
                {
                    bool shouldUnequip = _equipment.WeaponSlot.IsEmpty;

                    if (shouldUnequip == true)
                        _weaponState.Set(WeaponState.Unequipping);
                }
                break;

            case WeaponState.Unequipping:
                {
                    if (_timeSinceLastWeaponStateChange > 0.8f)
                        _weaponState.Set(WeaponState.NoWeapon);
                }
                break;
        }

        // Recoil
        _targetRecoilY = Mathf.Max(0f, _targetRecoilY -= Time.deltaTime * 45f);
        _currentRecoilY = Mathf.Lerp(_currentRecoilY, _targetRecoilY, Time.deltaTime * 50f);
        //_recoilY = Mathf.Lerp(_recoilY, 0f, Time.deltaTime * 5f);
        //_currentRecoilY = Mathf.MoveTowards(_currentRecoilY, _recoilY, 100f * Time.deltaTime);

        _targetRecoilX = Mathf.MoveTowards(_targetRecoilX, 0f, Time.deltaTime * 45f);
        _currentRecoilX = Mathf.Lerp(_currentRecoilX, _targetRecoilX, Time.deltaTime * 50f);

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
                _armsAnimator.SetBool("is_aiming", true);
            }
        }
        else
        {
            if (_currentInput.WantsAim == false || CanAim() == false)
            {
                _isAiming = false;
                _armsAnimator.SetBool("is_aiming", false);

                // Stop remaining recoil
                if (false)
                {
                    _cameraTargetRotY += _currentRecoilX;
                    _targetRecoilX = 0f;
                    _currentRecoilX = 0f;

                    _cameraTargetRotX -= _currentRecoilY;
                    _targetRecoilY = 0f;
                    _currentRecoilY = 0f;
                }
            }
        }

        // Field of view
        _currentFOV = Mathf.Lerp(_currentFOV, _isAiming ? _aimingFieldOfView : _fieldOfView, Time.deltaTime * 5f);

        // Update camera
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

        if (IsDead == false && _timeSinceLastDamage > 10f && CanRegnerateHealth() == true)
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

        playerInput.MouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity.Value * GetMouseSensetivityMultiplier();
        playerInput.MouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity.Value * GetMouseSensetivityMultiplier();

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

        //var yRotation = transform.eulerAngles.y + input.MouseX;

        //_cameraTargetRotY += input.MouseX + _currentRecoilX; permanent apply

        _cameraTargetRotY += input.MouseX;

        var currentMouseInput = input.MouseY;

        // If Y recoil is present
        if (_targetRecoilY > 0f)
        {
            // And we want to move camera down, we firstly decrese recoil
            if (input.MouseY < 0.0001f)
            {
                _targetRecoilY += input.MouseY;
            }

            // If we want to rotate camera UP, we transfer recoil onto rotation
            if (input.MouseY > 0.0001f)
            {
                // Don't know if this shit works
                _targetRecoilY -= input.MouseY;
                _cameraTargetRotX -= input.MouseY;
            }
        }
        else
        {
            _cameraTargetRotX -= input.MouseY;
        }

        _cameraTargetRotX = Mathf.Clamp(_cameraTargetRotX, -70f, 70f);

        var finalAngle = Mathf.Clamp(_cameraTargetRotX - _currentRecoilY, -70f, 70f);

        transform.eulerAngles = new Vector3(0f, _cameraTargetRotY + _currentRecoilX, 0f);
        _head.localEulerAngles = new Vector3(finalAngle, 0f, 0f);
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
        var woundedMultipler = Health / _maxHealth < 0.3f ? 0.6f : 1f;

        return baseSpeed * multipler * crouchMultipler * woundedMultipler;
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

        return true && _timeSinceLastPostureChange > _crouchAnimationDuration && IsDead == false;
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
        return
            IsDead == false &&
            _controller.isGrounded == true &&
            _equipment.WeaponSlot.IsEmpty == false &&
            _weaponState.Current == WeaponState.Ready;
    }

    public float GetMouseSensetivityMultiplier()
    {
        return _isAiming ? 0.5f : 1f;
    }

    public bool CanShoot()
    {
        return IsDead == false && _isAiming == true && _controller.isGrounded == true;
    }

    public bool CanRegnerateHealth()
    {
        return false;
    }

    public bool CanEquipWeapon()
    {
        return true;
    }

    public override Vector3 GetCameraPosition() => new Vector3(_head.transform.position.x, _currentCameraHeight, _head.transform.position.z);
    public override Quaternion GetCameraRotation() => _head.rotation;
    public override bool OverrideCameraFOV => true;
    public override float GetCameraFOV() => _currentFOV;

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

    private enum WeaponState
    {
        NoWeapon,
        Equipping,
        Ready,
        Unequipping
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

public sealed class EnumState<T> where T : Enum
{

    private readonly Action<T> _stateChangedAction;

    public EnumState(Action<T> stateChangeEvent)
    {
        _stateChangedAction = stateChangeEvent;
    }

    public T Current { get; private set; }

    public void Set(T newState)
    {
        Current = newState;
        _stateChangedAction.Invoke(Current);
    }

}