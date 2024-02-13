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
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _maxHealth = 5f;

    private CharacterController _controller;
    private Vector3 _velocityXZ;
    private float _velocityY;
    private TimeSince _timeSinceLastDeath = new TimeSince(float.NegativeInfinity);
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private readonly List<CharacterModifier> _modifiers = new List<CharacterModifier>();

    public PlayerInteraction Interactor => _interactor;
    public Inventory Inventory => _inventory;
    public bool IsDead { get; private set; }
    public float RespawnTime => _respawnTime;
    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Health = _maxHealth;

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Kill(DeathType type)
    {
        IsDead = true;
        _timeSinceLastDeath = new TimeSince(Time.time);
        Died?.Invoke(type);
        GetComponent<Animator>()?.SetBool("dead", true);
        _modifiers.Clear();
    }

    public void Respawn()
    {
        IsDead = false;
        Respawned?.Invoke();
        GetComponent<Animator>()?.SetBool("dead", false);
        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;
        _head.localRotation = Quaternion.identity;
        Health = _maxHealth;
    }

    public void ApplyModifier(CharacterModifier modifier, float duration)
    {
        modifier.Init(duration);
        _modifiers.Add(modifier);
    }

    public void ApplyDamage(float damage)
    {
        if (IsDead == true)
            return;

        Health = Mathf.Max(0f, Health - damage);
        Damaged?.Invoke();

        if (Health <= 0f)
        {
            Kill(DeathType.Psionic);
        }
    }

    public override void PossessedTick()
    {
        if (IsDead == true)
        {
            UpdateDead();
        }
        else
        {
            var input = GatherInput();
            UpdateAlive(input);
        }
    }

    private PlayerInput GatherInput()
    {
        var playerInput = new PlayerInput();

        playerInput.MouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * 100f;
        playerInput.MouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * 100f;

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
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            var modifier = _modifiers[i];

            if (modifier.TimeUntilExpires < 0)
            {
                _modifiers.RemoveAt(i);
            }
        }

        UpdateRotation(input);
        UpdateMovement(input);

        if (CanInteract() == true && input.WantsInteract == true)
        {
            _interactor.TryPerform(input.InteractionIndex);
        }

        const bool canThrowItems = false;
        if (Input.GetKeyDown(KeyCode.Q) == true && canThrowItems == true)
        {
            var items = _inventory.Content;
            if (items.Length > 0)
            {
                var item = items[0];
                _inventory.RemoveItem(item);
                item.transform.position = _head.position;
                item.Push(_head.forward * 250f + _velocityXZ * 45f);
            }
        }
    }

    private void UpdateRotation(PlayerInput input)
    {
        if (CanRotateHead() == false)
            return;

        var yRotation = transform.eulerAngles.y + input.MouseX * Time.deltaTime;
        var xRotation = _head.localEulerAngles.x - input.MouseY * Time.deltaTime;
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

        return true;
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
            var modifierMultipler = modifier.GetSpeedMultipler();
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
    public TimeUntil TimeUntilExpires { get; private set; }

    public void Init(float duration)
    {
        TimeUntilExpires = new TimeUntil(Time.time + duration);
    }

    public virtual float GetSpeedMultipler() => 1f;
    public virtual bool CanInteract() => true;
    public virtual bool CanJump() => true;

}
