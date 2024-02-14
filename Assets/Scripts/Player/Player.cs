using System;
using UnityEngine;

public sealed class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _hud;

    private Pawn _currentPawn;

    private void OnEnable()
    {
        _character.Damaged += OnCharacterDamaged;
        _character.Died += OnCharacterDied;
        _character.Respawned += OnCharacterRespawned;
    }

    private void OnDisable()
    {
        _character.Damaged -= OnCharacterDamaged;
        _character.Died -= OnCharacterDied;
        _character.Respawned -= OnCharacterRespawned;
    }

    private void Start()
    {
        Possess(_character);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            if (_currentPawn != _character)
            {
                Unpossess();
            }
        }

        if (_currentPawn != null)
        {
            if (_currentPawn.WantsUnpossess == false)
            {
                _currentPawn.PossessedTick();
                _mainCamera.transform.SetPositionAndRotation(
                    _currentPawn.GetCameraPosition(), 
                    _currentPawn.GetCameraRotation());
            }
            else
            {
                if (_currentPawn != _character)
                {
                    Unpossess();
                }
            }
        }
    }

    public void Possess(Pawn pawn)
    {
        if (_currentPawn == pawn)
            return;

        _currentPawn = pawn;
        _currentPawn.OnPossessed(this);

        _hud.SetActive(pawn == _character);
    }

    public void Unpossess()
    {
        if (_currentPawn != null && _currentPawn != _character)
        {
            _currentPawn.OnUnpossessed();
            Possess(_character);
        }
    }

    private void OnCharacterDamaged()
    {
        Possess(_character);
    }

    private void OnCharacterDied(DeathType deathType)
    {
        Possess(_character);
        _hud.SetActive(false);
    }

    private void OnCharacterRespawned()
    {
        _hud.SetActive(true);
    }


}
