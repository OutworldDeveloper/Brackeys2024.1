using UnityEngine;

public sealed class ToggleDoorInteraction: Interaction
{

    [SerializeField] private Door _door;
    [SerializeField] private bool _canClose = true;

    public override string Text => _door.IsOpen ? "Close" : "Open";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        if (_door.IsOpen == true && _canClose == false)
            return false;

        return base.IsAvaliable(player);
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_door.IsOpen == true && _canClose == true)
        {
            _door.Close();
            return;
        }

        _door.TryOpen(player);
    }

}
