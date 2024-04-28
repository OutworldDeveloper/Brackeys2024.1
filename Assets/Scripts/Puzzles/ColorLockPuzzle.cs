using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorLockPuzzle : Pawn
{

    [SerializeField] private Door _door;
    [SerializeField] private ColorLockRotator[] _rotators;
    [SerializeField] private Transform _camera;

    private int _selectedRotator;

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

    public override void InputTick()
    {
        int previousSelectedRotator = _selectedRotator;

        if (Input.GetKeyDown(KeyCode.A) == true)
            _selectedRotator--;

        if (Input.GetKeyDown(KeyCode.D) == true)
            _selectedRotator++;

        if (_selectedRotator < 0)
            _selectedRotator = 0;

        if (_selectedRotator == _rotators.Length)
            _selectedRotator = _rotators.Length - 1;

        if (Input.GetKeyDown(KeyCode.W) == true)
            _rotators[_selectedRotator].RotateUp();

        if (Input.GetKeyDown(KeyCode.S) == true)
            _rotators[_selectedRotator].RotateDown();
    }

}
