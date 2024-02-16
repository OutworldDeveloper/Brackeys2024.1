using System;
using UnityEngine;

public sealed class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _pauseMenu;

    private bool _isPauseMenuOpen;
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
        if (Input.GetKeyDown(KeyCode.Tab) == true)
            HandleEscapeButton();

        if (_isPauseMenuOpen == true)
            return;

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

    private void HandleEscapeButton()
    {
        if (_isPauseMenuOpen == true)
        {
            ClosePauseMenu();
            return;
        }

        if (_currentPawn != _character && _currentPawn.CanUnpossessAtWill() == true)
        {
            Unpossess();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void Possess(Pawn pawn)
    {
        if (_currentPawn == pawn)
            return;

        _currentPawn?.OnUnpossessed();
        _currentPawn = pawn;
        _currentPawn.OnPossessed(this);

        if (_currentPawn.CanUnpossessAtWill() && _currentPawn != _character)
        {
            Notification.Show("Press Tab to exit", 2f);
        }

        UpdateState();
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
        UpdateState();
    }

    private void OnCharacterRespawned()
    {
        UpdateState();
    }

    public void OpenPauseMenu()
    {
        _isPauseMenuOpen = true;
        UpdateState();
    }

    public void ClosePauseMenu()
    {
        _isPauseMenuOpen = false;
        UpdateState();

        PlayerPrefs.Save();
    }

    private void UpdateState()
    {
        bool showHud = _isPauseMenuOpen == false && _character.IsDead == false && _currentPawn == _character;

        _hud.SetActive(showHud);
        _pauseMenu.SetActive(_isPauseMenuOpen == true);

        bool showCursor = _isPauseMenuOpen == true;

        Cursor.visible = showCursor ? true : false;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        bool pauseGame = _isPauseMenuOpen == true;

        Time.timeScale = pauseGame ? 0f : 1f;
    }

}
