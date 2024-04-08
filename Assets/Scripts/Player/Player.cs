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
    [SerializeField] private bool _smoothPawnCameraChange;
    [SerializeField] private Prefab<UI_PauseMenu> _pauseMenu;
    [SerializeField] private Prefab<UI_InventoryScreen> _inventoryScreen;
    [SerializeField] private Prefab<UI_Panel> _deathScreen;
    [SerializeField] private Prefab<UI_InventorySelectScreen> _itemSelectionScreen;
    [SerializeField] private Prefab<UI_SaveGameWindow> _saveGameScreen;

    [SerializeField] private Volume _blurVolume;

    [SerializeField] private UI_PanelsManager _panels;

    private Pawn _currentPawn;
    private TimeSince _timeSincePawnChanged;

    public UI_PanelsManager Panels => _panels;

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
        UpdateState(); // Хы

        if (Input.GetKeyDown(KeyCode.Escape) == true)
            HandleEscapeButton();

        if (_panels.HasActivePanel == true)
        {
            _panels.Active.InputUpdate();
            return;
        }    

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
        if (_panels.HasActivePanel == true)
        {
            _panels.TryCloseActivePanel();
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
        Delayed.Do(() => _panels.InstantiateAndOpenFrom(_deathScreen), 2.75f);
    }

    private void OnCharacterRespawned()
    {
        UpdateState();
    }

    public void OpenPauseMenu()
    {
        _panels.InstantiateAndOpenFrom(_pauseMenu);
        UpdateState();
    }

    private void UpdateState()
    {
        bool showHud = 
            _panels.HasActivePanel == false &&
            _character.IsDead == false && 
            _currentPawn == _character;

        _hud.SetActive(showHud);

        bool showCursor = _panels.HasActivePanel == true || _currentPawn.ShowCursor == true;

        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        bool pauseGame = _panels.HasActivePanel == true;

        Time.timeScale = pauseGame ? 0f : 1f;
    }

    public UI_InventoryScreen OpenInventory()
    {
        var inventoryScreen = _panels.InstantiateAndOpenFrom(_inventoryScreen);
        inventoryScreen.SetTarget(_character);
        return inventoryScreen;
    }

    public UI_InventorySelectScreen OpenItemSelection(IItemSelector selector)
    {
        var selectionScreen = Panels.InstantiateAndOpenFrom(_itemSelectionScreen);
        selectionScreen.SetTarget(_character);
        selectionScreen.SetSelector(selector);
        return selectionScreen;
    }

    public UI_SaveGameWindow OpenSaveScreen()
    {
        return Panels.InstantiateAndOpenFrom(_saveGameScreen);
    }

}
