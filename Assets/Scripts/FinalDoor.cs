using UnityEngine;

public sealed class FinalDoor : MonoBehaviour
{

    [SerializeField] private Code _code;
    [SerializeField] private char _test;
    [SerializeField] private Door _door;
    [SerializeField] private MoviePawn _moviePawn;

    private char[] _input;

    public bool IsOpen { get; private set; }

    private void Start()
    {
        var codeLenght = _code.Value.ToString().Length;
        _input = new char[codeLenght];

        for (int i = 0; i < _input.Length; i++)
        {
            _input[i] = '6';
        }

        _door.Block();
    }

    public void Submit(string s)
    {
        SubmitCharacter(s[0]);
    }

    public void SubmitCharacter(char character)
    {
        for (int i = 0; i < _input.Length - 1; i++)
        {
            _input[i] = _input[i + 1];
        }

        _input[_input.Length - 1] = character;

        Notification.ShowDebug(new string(_input), 5f);

        SubmitCode(new string(_input));
    }

    private void SubmitCode(string code)
    {
        if (code != _code.Value)
            return;

        if (IsOpen == true)
            return;

        Notification.ShowDebug("Success!");

        IsOpen = true;
        Delayed.Do(_door.Open, 0.5f);
        FindObjectOfType<Player>().Possess(_moviePawn);
    }

}
