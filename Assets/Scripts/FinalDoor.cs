using UnityEngine;

public sealed class FinalDoor : MonoBehaviour
{

    [SerializeField] private FinalDoorCode _code;
    [SerializeField] private Door _door;
    [SerializeField] private MoviePawn _moviePawn;

    private CodeCharacter[] _input;

    public bool IsOpen { get; private set; }

    private void Start()
    {
        var code = _code.Characters;
        _input = new CodeCharacter[code.Length];

        for (int i = 0; i < _input.Length; i++)
        {
            _input[i] = CodeCharacter.A;
        }

        _door.Block();
    }

    public void SubmitCharacter(CodeCharacter character)
    {
        for (int i = 0; i < _input.Length - 1; i++)
        {
            _input[i] = _input[i + 1];
        }

        _input[_input.Length - 1] = character;

        Notification.ShowDebug(character.ToString(), 5f);

        SubmitCode();
    }

    private void SubmitCode()
    {
        if (_code.IsValid(_input) == false)
            return;

        if (IsOpen == true)
            return;

        Notification.ShowDebug("Success!");

        IsOpen = true;
        Delayed.Do(() =>
        {
            _door.TryOpen(null, true);
            FindObjectOfType<Player>().Possess(_moviePawn);
        }, 1.25f);
    }

}
