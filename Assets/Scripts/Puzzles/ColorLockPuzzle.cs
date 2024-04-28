using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorLockPuzzle : Pawn
{

    [SerializeField] private Door _door;
    [SerializeField] private ColorLockRotator[] _rotators;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Transform _camera;
    [SerializeField] private MoviePawn _successPawn;

    private int _selectedRotator;
    private TimeUntil _timeUntilInputAvaliable;

    private void Start()
    {
        _door.Block();
    }

    public override Vector3 GetCameraPosition()
    {
        return _camera.position;
    }

    public override Quaternion GetCameraRotation()
    {
        return _camera.rotation;
    }

    public override void OnPossessed(Player player)
    {
        base.OnPossessed(player);
        _selectedRotator = _rotators.Length - 1;
        _rotators[_selectedRotator].SetMaterial(_selectedMaterial);
    }

    public override void OnUnpossessed()
    {
        base.OnUnpossessed();
        _rotators[_selectedRotator].SetMaterial(_defaultMaterial);
    }

    public override void InputTick()
    {
        if (_timeUntilInputAvaliable > 0)
            return;

        int previousSelectedRotator = _selectedRotator;

        if (Input.GetKeyDown(KeyCode.S) == true)
            _selectedRotator--;

        if (Input.GetKeyDown(KeyCode.W) == true)
            _selectedRotator++;

        if (_selectedRotator < 0)
            _selectedRotator = 0;

        if (_selectedRotator == _rotators.Length)
            _selectedRotator = _rotators.Length - 1;

        if (Input.GetKeyDown(KeyCode.A) == true)
        {
            _rotators[_selectedRotator].RotateUp();
            BlockInputFor(0.2f);
        }

        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            _rotators[_selectedRotator].RotateDown();
            BlockInputFor(0.2f);
        }

        if (Input.GetKeyDown(KeyCode.F) == true)
        {
            TryUnlock();
            BlockInputFor(0.3f);
        }

        if (previousSelectedRotator != _selectedRotator)
        {
            OnRotatorSelectionChanged(previousSelectedRotator, _selectedRotator);
            BlockInputFor(0.2f);
        }
    }

    private void BlockInputFor(float duration)
    {
        _timeUntilInputAvaliable = new TimeUntil(Time.time * duration);
    }

    private void TryUnlock()
    {
        bool shouldUnlock = true;

        foreach (var rotator in _rotators)
        {
            if (rotator.IsRightDigit == true)
                continue;

            shouldUnlock = false;
            break;
        }

        if (shouldUnlock == false)
        {
            _door.TryOpen();
            return;
        }

        _door.Unblock();
        _door.TryOpen();
        Player.Possess(_successPawn);
    }

    private void OnRotatorSelectionChanged(int previous, int current)
    {
        _rotators[previous].SetMaterial(_defaultMaterial);
        _rotators[current].SetMaterial(_selectedMaterial);
    }

}
