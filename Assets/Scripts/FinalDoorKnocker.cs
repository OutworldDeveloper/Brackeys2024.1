using UnityEngine;

public sealed class FinalDoorKnocker : MonoBehaviour
{

    [SerializeField] private FinalDoor _finalDoor;
    [SerializeField] private char _char;
    [SerializeField] private Door _target;

    private void OnEnable()
    {
        _target.SomeoneKnocked += OnDoorKnocked;
    }

    private void OnDisable()
    {
        _target.SomeoneKnocked -= OnDoorKnocked;
    }

    private void OnDoorKnocked()
    {
        _finalDoor.SubmitCharacter(_char);
    }

}
