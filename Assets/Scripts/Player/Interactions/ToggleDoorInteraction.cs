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

        if (_door.IsLocked == false)
        {
            _door.Open();
            return;
        }

        if (player.Inventory.HasItem(_door.Key) == true)
        {
            _door.Open();
            return;
        }

        _door.PlayLockedSound();
        Notification.Do("Locked!");
    }

}
