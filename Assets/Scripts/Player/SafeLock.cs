﻿using System;
using UnityEngine;

public sealed class SafeLock : Pawn
{

    [SerializeField] private Door _targetDoor;
    [SerializeField] private Transform _head;
    [SerializeField] private Code _code;
    [SerializeField] private LockButton[] _buttons;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _pressSound;

    private int _selectedButtonIndex = -1;
    private TimeSince _timeSinceLastPress;
    private string _currentEnteredCode;

    public bool IsOpen { get; private set; }

    public override Vector3 GetCameraPosition() => _head.position;
    public override Quaternion GetCameraRotation() => _head.rotation;

    private void Start()
    {
        _targetDoor.Block();
    }

    public override void OnPossessed(Player player)
    {
        base.OnPossessed(player);
        SelectButton(0);
    }

    public override void OnUnpossessed()
    {
        base.OnUnpossessed();
        SelectButton(-1);
    }

    public override void PossessedTick()
    {
        if (_timeSinceLastPress < 0.5f)
            return;

        Vector2Int moveSelection = new Vector2Int()
        {
            x = Input.GetKeyDown(KeyCode.A) ? -1 : Input.GetKeyDown(KeyCode.D) ? 1 : 0,
            y = Input.GetKeyDown(KeyCode.W) ? -1 : Input.GetKeyDown(KeyCode.S) ? 1 : 0,
        };

        int newSelection = _selectedButtonIndex + moveSelection.x + moveSelection.y * 3;
        //newSelection = Mathf.Clamp(newSelection, 0, _buttons.Length - 1);
        if (newSelection >= 0 && newSelection < _buttons.Length)
        {
            SelectButton(newSelection);
        }

        if (Input.GetKeyDown(KeyCode.F) == true || Input.GetKeyDown(KeyCode.Space) == true || Input.GetKeyDown(KeyCode.Return) == true)
        {
            PressSelectedButton();
        }
    }

    private void PressSelectedButton()
    {
        switch (_selectedButtonIndex)
        {
            case 9:
                _currentEnteredCode = string.Empty;
                break;
            case 10:
                _currentEnteredCode += "0";
                Notification.Do(_currentEnteredCode.ToString(), 0.5f);
                break;
            case 11:
                Notification.Do(_currentEnteredCode.ToString(), 0.5f);
                SubmitCode(_currentEnteredCode);
                _currentEnteredCode = string.Empty;
                break;
            default:
                _currentEnteredCode += _selectedButtonIndex + 1;
                Notification.Do(_currentEnteredCode.ToString(), 0.5f);
                break;
        }

        _buttons[_selectedButtonIndex].OnPressed();
        _pressSound.Play(_audioSource);
        _timeSinceLastPress = new TimeSince(Time.time);
    }

    private void SubmitCode(string code)
    {
        if (code == _code.Value.ToString())
        {
            _targetDoor.Unblock();
            IsOpen = true;
            _targetDoor.Open();
            Unpossess();
        }
    }

    private void SelectButton(int index)
    {
        if (_selectedButtonIndex != -1)
            _buttons[_selectedButtonIndex].OnDeselected();

        _selectedButtonIndex = index;

        if (_selectedButtonIndex != -1)
            _buttons[_selectedButtonIndex].OnSelected();
    }

}
