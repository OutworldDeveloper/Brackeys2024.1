using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-1)]
public sealed class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Prefab<UI_PauseMenu> _pauseMenu;
    [SerializeField] private Prefab<UI_InventoryScreen> _inventoryScreen;
    [SerializeField] private Prefab<UI_Panel> _deathScreen;
    [SerializeField] private Prefab<UI_InventorySelectScreen> _itemSelectionScreen;
    [SerializeField] private Prefab<UI_SaveGameWindow> _saveGameScreen;

    [SerializeField] private Volume _blurVolume;
    [SerializeField] private UI_PanelsManager _panels;

    [SerializeField] private AnimationCurve _cameraTransitionCurve;

    private CameraState _currentState;
    private CameraState _lastCameraState;

    public UI_PanelsManager Panels => _panels;
    public PawnStack PawnStack { get; private set; }

    private void Awake()
    {
        PawnStack = new PawnStack(this, _character);
        PawnStack.ActivePawnChanged += OnActivePawnChanged;
        _character.Damaged += OnCharacterDamaged;
        _character.Died += OnCharacterDied;
    }

    private void Start()
    {

    }

    private void Update()
    {
        UpdateState();

        if (Input.GetKeyDown(KeyCode.Escape) == true)
            HandleEscapeButton();

        if (_panels.HasActivePanel == true)
        {
            _panels.Active.InputUpdate();
            return;
        }

        PawnStack.ActivePawn.InputTick();
    }

    private void LateUpdate()
    {
        if (PawnStack.ActivePawn.CameraTransition == CameraTransition.Fade && PawnStack.TimeSinceLastActivePawnChange < 0.2f)
            return;

        CameraState cameraState = PawnStack.ActivePawn.GetCameraState();

        if (PawnStack.ActivePawn.CameraTransition == CameraTransition.Move && PawnStack.TimeSinceLastActivePawnChange < 0.4f)
        {
            float t = PawnStack.TimeSinceLastActivePawnChange / 0.4f;
            t = _cameraTransitionCurve.Evaluate(t);
            cameraState = CameraState.Lerp(_lastCameraState, cameraState, t);
        }

        ApplyCameraState(cameraState);

        _blurVolume.enabled = PawnStack.ActivePawn.GetBlurStatus(out float targetBlurDistance);

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

        if (PawnStack.IsStackEmpty == false && PawnStack.ActivePawn.CanRemoveAtWill() == true)
        {
            PawnStack.Remove(PawnStack.ActivePawn);
        }
        else
        {
            _panels.InstantiateAndOpenFrom(_pauseMenu);
        }
    }

    private void OnActivePawnChanged(Pawn activePawn)
    {
        _lastCameraState = _currentState;

        if (activePawn.CameraTransition == CameraTransition.Fade)
            ScreenFade.FadeOutFor(0.2f);
    }

    private void OnCharacterDamaged()
    {
        PawnStack.RemoveAll();
    }

    private void OnCharacterDied()
    {
        Delayed.Do(() => _panels.InstantiateAndOpenFrom(_deathScreen), 2.75f);
    }

    private void UpdateState()
    {
        // Hud
        bool showHud =
            _panels.HasActivePanel == false &&
            _character.IsDead == false &&
            PawnStack.IsStackEmpty == true;

        _hud.SetActive(showHud);

        // Cursor
        bool showCursor = _panels.HasActivePanel == true || PawnStack.ActivePawn.ShowCursor == true;

        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        // Pause
        bool pauseGame = _panels.RequirePause();

        Time.timeScale = pauseGame ? 0f : 1f;
    }

    private void ApplyCameraState(CameraState state)
    {
        _mainCamera.transform.SetPositionAndRotation(state.Position, state.Rotation);
        _mainCamera.fieldOfView = state.FieldOfView;

        _currentState = state;
    }

    public UI_InventoryScreen OpenInventory()
    {
        var inventoryScreen = _panels.InstantiateAndOpenFrom(_inventoryScreen);
        inventoryScreen.SetTarget(_character);
        return inventoryScreen;
    }

    public UI_InventorySelectScreen OpenItemSelection(ItemSelector selector)
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

public sealed class PawnStack
{

    public event Action<Pawn> ActivePawnChanged;

    private readonly Player _player;
    private readonly Pawn _defaultPawn;
    private readonly List<Pawn> _stack = new List<Pawn>();

    public PawnStack(Player player, Pawn defaultPawn)
    {
        _player = player;
        _defaultPawn = defaultPawn;
        _defaultPawn.OnAddedToStack(player);
        _defaultPawn.OnReceivePlayerControl();
    }

    public Pawn ActivePawn => _stack.Count > 0 ? _stack[^1] : _defaultPawn;
    public bool IsStackEmpty => _stack.Count == 0;
    public TimeSince TimeSinceLastActivePawnChange { get; private set; } = TimeSince.Never;

    public void Push(Pawn pawn)
    {
        ActivePawn.OnLostPlayerControl();

        _stack.Add(pawn);
        pawn.OnAddedToStack(_player);
        pawn.OnReceivePlayerControl();

        TimeSinceLastActivePawnChange = TimeSince.Now();
        ActivePawnChanged?.Invoke(ActivePawn);
    }

    public void Remove(Pawn pawn)
    {
        bool removingActivePawn = pawn == ActivePawn;

        if (removingActivePawn == true)
        {
            pawn.OnLostPlayerControl();
        }

        _stack.Remove(pawn);
        pawn.OnRemovedFromStack(_player);

        if (removingActivePawn == true)
        {
            ActivePawn.OnReceivePlayerControl();
            TimeSinceLastActivePawnChange = TimeSince.Now();
            ActivePawnChanged?.Invoke(ActivePawn);
        }
    }

    public void RemoveAll()
    {
        if (IsStackEmpty == true)
            return;

        ActivePawn.OnLostPlayerControl();
        ActivePawn.OnRemovedFromStack(_player);

        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            _stack[i].OnRemovedFromStack(_player);
        }

        ActivePawn.OnReceivePlayerControl();

        TimeSinceLastActivePawnChange = TimeSince.Now();
        ActivePawnChanged?.Invoke(ActivePawn);
    }

}
