using System;
using UnityEngine;

public sealed class SafeLock : Pawn
{

    [SerializeField] private Door _targetDoor;
    [SerializeField] private Code _code;
    [SerializeField] private LockButton[] _buttons;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _pressSound;
    [SerializeField] private Sound _selectSound;

    private int _selectedButtonIndex = -1;
    private TimeSince _timeSinceLastPress;
    private string _currentEnteredCode;

    public bool IsOpen { get; private set; }

    private void Start()
    {
        _targetDoor.Block();

        RegisterAction(new PawnAction("Select", KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D));
        RegisterAction(new PawnAction("Press", KeyCode.F));
        RegisterAction(new PawnAction("Back", KeyCode.Escape));
    }

    public override void OnReceivePlayerControl()
    {
        base.OnReceivePlayerControl();
        SelectButton(0, false);
    }

    public override void OnLostPlayerControl()
    {
        base.OnLostPlayerControl();
        SelectButton(-1);
        _currentEnteredCode = string.Empty;
    }

    public override void InputTick()
    {
        if (_timeSinceLastPress < 0.2f)
            return;

        Vector2Int moveSelection = new Vector2Int()
        {
            x = Input.GetKeyDown(KeyCode.A) ? -1 : Input.GetKeyDown(KeyCode.D) ? 1 : 0,
            y = Input.GetKeyDown(KeyCode.W) ? -1 : Input.GetKeyDown(KeyCode.S) ? 1 : 0,
        };

        int newSelection = _selectedButtonIndex + moveSelection.x + moveSelection.y * 3;
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
        _timeSinceLastPress = new TimeSince(Time.time);
        _buttons[_selectedButtonIndex].OnPressed();
        _pressSound.Play(_audioSource);

        switch (_selectedButtonIndex)
        {
            case 9:
                _currentEnteredCode = string.Empty;
                break;
            case 10:
                _currentEnteredCode += "0";
                Notification.Show(_currentEnteredCode.ToString(), 0.5f);
                break;
            case 11:
                SubmitCode(_currentEnteredCode);
                _currentEnteredCode = string.Empty;
                break;
            default:
                _currentEnteredCode += _selectedButtonIndex + 1;
                Notification.Show(_currentEnteredCode.ToString(), 0.5f);
                break;
        }
    }

    private void SubmitCode(string code)
    {
        if (code == _code.Value.ToString())
        {
            _targetDoor.Unblock();
            IsOpen = true;
            _targetDoor.Open();
            RemoveFromStack();
        }
    }

    private void SelectButton(int index, bool playSound = true)
    {
        if (_selectedButtonIndex == index)
            return;

        if (_selectedButtonIndex != -1)
            _buttons[_selectedButtonIndex].OnDeselected();

        _selectedButtonIndex = index;

        if (_selectedButtonIndex != -1)
        {
            if (playSound == true)
                _selectSound?.Play(_audioSource);

            _buttons[_selectedButtonIndex].OnSelected();
        }
    }

}
