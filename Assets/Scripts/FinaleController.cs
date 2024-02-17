using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FinaleController : Pawn
{

    [SerializeField] private PlayerTrigger _finalTrigger;
    [SerializeField] private Transform _cameraTransform;

    private bool _isFinished;

    private void OnEnable()
    {
        _finalTrigger.EnterEvent.AddListener(PlayerEntered);
    }

    private void OnDisable()
    {
        _finalTrigger.EnterEvent.RemoveListener(PlayerEntered);
    }

    private void PlayerEntered(PlayerCharacter player)
    {
        if (_isFinished == true)
            return;

        _isFinished = true;
        //player.Player.Possess(this);
        player.ApplyModifier(new FinalModifier(), -1);

        Delayed.Do(() => 
        {
            Notification.Show("Thanks for playing!", 5f);
            Delayed.Do(() => SceneManager.LoadScene(0), 5f);
        }, 2f);
    }

    public override Vector3 GetCameraPosition()
    {
        return _cameraTransform.position;
    }

    public override Quaternion GetCameraRotation()
    {
        return _cameraTransform.rotation;
    }

}

public sealed class FinalModifier : CharacterModifier
{

    public override bool CanInteract()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0f;
    }

    public override bool CanRotateCamera()
    {
        return false;
    }

}
