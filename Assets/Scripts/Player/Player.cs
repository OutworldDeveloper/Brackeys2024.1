using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class Player : MonoBehaviour
{

    public event Action<Pawn> PawnChanged;

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _hud;
    [SerializeField] private UI_PauseMenu _pauseMenu;
    [SerializeField] private bool _smoothPawnCameraChange;

    [SerializeField] private Volume _blurVolume;

    private bool _isPauseMenuOpen;
    private Pawn _currentPawn;
    private TimeSince _timeSincePawnChanged;

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
        OpenPauseMenu(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) == true || Input.GetKeyDown(KeyCode.Escape))
            HandleEscapeButton();

        if (_isPauseMenuOpen == true)
            return;

        if (_currentPawn != null)
        {
            if (_currentPawn.WantsUnpossess == false)
            {
                _currentPawn.InputTick();
                _currentPawn.PossessedTick();
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

    private void LateUpdate()
    {
        if (_currentPawn == null)
            return;

        if (_smoothPawnCameraChange == true && _timeSincePawnChanged < 0.2f)
            return;

        if (_currentPawn.OverrideCameraPositionAndRotation == true)
        {
            _mainCamera.transform.SetPositionAndRotation(
                _currentPawn.GetCameraPosition(),
                _currentPawn.GetCameraRotation());
        }

        if (_currentPawn.OverrideCameraFOV == true)
        {
            _mainCamera.fieldOfView = _currentPawn.GetCameraFOV();
        }

        _blurVolume.enabled = _currentPawn.GetBlurStatus(out float targetBlurDistance);

        // Blur
        if (_blurVolume.profile.TryGet(out DepthOfField dof))
        {
            dof.focusDistance.Override(targetBlurDistance);
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
            OpenPauseMenu(false);
        }
    }

    public void Possess(Pawn pawn)
    {
        if (_currentPawn == pawn)
            return;

        // Not sure but this could fix jittering
        if (_currentPawn != null && pawn.OverrideCameraPositionAndRotation == false)
        {
            _mainCamera.transform.SetPositionAndRotation(
                _currentPawn.GetCameraPosition(),
                _currentPawn.GetCameraRotation());
        }
        // end

        _currentPawn?.OnUnpossessed();
        _currentPawn = pawn;
        _currentPawn.OnPossessed(this);

        if (_currentPawn.CanUnpossessAtWill() && _currentPawn != _character)
        {
            //Notification.Show("Press Tab to exit", 2f);
        }

        UpdateState();
        _timeSincePawnChanged = new TimeSince(Time.time);

        if (_smoothPawnCameraChange == true)
            ScreenFade.FadeOutFor(0.2f);

        PawnChanged?.Invoke(_currentPawn);
    }

    public void Unpossess()
    {
        if (_currentPawn != null && _currentPawn != _character)
        {
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

    public void OpenPauseMenu(bool menu)
    {
        _pauseMenu.SetMode(menu);
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
        _pauseMenu.gameObject.SetActive(_isPauseMenuOpen == true);

        bool showCursor = _isPauseMenuOpen == true || _currentPawn.ShowCursor == true;

        Cursor.visible = showCursor ? true : false;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        bool pauseGame = _isPauseMenuOpen == true;

        Time.timeScale = pauseGame ? 0f : 1f;
    }

}
