using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController), typeof(PlayerInteraction))]
public sealed class PlayerCharacter : Pawn
{

    public event Action<DeathType> Died;
    public event Action Respawned;
    public event Action Damaged;

    [SerializeField] private Transform _head;
    [SerializeField] private PlayerInteraction _interactor;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private FloatParameter _mouseSensitivity;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _maxHealth = 5f;
    [SerializeField] private bool _allowJumping;

    private CharacterController _controller;
    private Vector3 _velocityXZ;
    private float _velocityY;
    private TimeSince _timeSinceLastDeath = new TimeSince(float.NegativeInfinity);
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private readonly List<CharacterModifier> _modifiers = new List<CharacterModifier>();
    private TimeSince _timeSinceLastDamage = new TimeSince(float.NegativeInfinity);
    private PlayerInput _currentInput;

    public PlayerInteraction Interactor => _interactor;
    public Inventory Inventory => _inventory;
    public bool IsDead { get; private set; }
    public float RespawnTime => _respawnTime;
    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }
    public Vector3 HorizontalVelocity => _velocityXZ;
    public bool IsGrounded => _controller.isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Health = _maxHealth;

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        ApplyModifier(new SpawnBlockModifier(), 0.4f);
    }

    private void Update()
    {
        if (IsDead == true)
        {
            UpdateDead();
        }
        else
        {
            UpdateAlive(_currentInput);
        }
    }

    public override void OnUnpossessed()
    {
        base.OnUnpossessed();

        _currentInput = new PlayerInput()
        {
            WantsJump = false,
            InteractionIndex = -1,
            Direction = FlatVector.zero,
        };
        _velocityXZ = Vector3.zero;
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

        //_controller.enabled = false;
        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;
        Physics.SyncTransforms();
        //_controller.enabled = true;
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

    public override void PossessedTick()
    {
        _currentInput = GatherInput();
    }

    private PlayerInput GatherInput()
    {
        var playerInput = new PlayerInput();

        playerInput.MouseX = Input.GetAxis("Mouse X") * _mouseSensitivity.Value;
        playerInput.MouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity.Value;

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

        return playerInput;
    }

    private void UpdateDead()
    {
        if (_timeSinceLastDeath > _respawnTime)
            Respawn();
    }

    private void UpdateAlive(PlayerInput input)
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

        // Regeneration
        if (_timeSinceLastDamage > 10f)
        {
            Health = Mathf.Min(Health + Time.deltaTime, _maxHealth);
        }

        UpdateRotation(input);
        UpdateMovement(input);

        if (CanInteract() == true && input.WantsInteract == true)
        {
            _interactor.TryPerform(input.InteractionIndex);
        }
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

    private bool CanRotateHead()
    {
        foreach (var modifier in _modifiers)
        {
            if (modifier.CanRotateCamera() == false)
                return false;
        }

        return true;
    }

    private bool CanInteract()
    {
        foreach (var modifier in _modifiers)
        {
            if (modifier.CanInteract() == false)
                return false;
        }

        return true;
    }

    private bool CanJump()
    {
        foreach (var modifier in _modifiers)
        {
            if (modifier.CanJump() == false)
                return false;
        }

        return _allowJumping;
    }

    private bool CanWalk()
    {
        return true;
    }

    private float GetSpeed()
    {
        var multipler = 1f;

        foreach (var modifier in _modifiers)
        {
            var modifierMultipler = modifier.GetSpeedMultiplier();
            multipler = Mathf.Min(multipler, modifierMultipler);
        }

        multipler = Mathf.Max(0f, multipler);
        return _speed * multipler;
    }

    public override Vector3 GetCameraPosition()
    {
        return _head.position;
    }

    public override Quaternion GetCameraRotation()
    {
        return _head.rotation;
    }

    private struct PlayerInput
    {
        public float MouseX;
        public float MouseY;
        public FlatVector Direction;
        public bool WantsJump;
        public int InteractionIndex;
        public bool WantsInteract => InteractionIndex != -1;
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

    public override float GetSpeedMultiplier()
    {
        return 0f;
    }

}
