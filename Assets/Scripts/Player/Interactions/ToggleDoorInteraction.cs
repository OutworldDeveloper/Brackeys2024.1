using UnityEngine;

public sealed class ToggleDoorInteraction: Interaction
{

    [SerializeField] private Door _door;

    public override string Text => _door.IsOpen ? "Close" : "Open";

    public override void Perform(PlayerCharacter player)
    {
        if (_door.IsOpen == true)
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

        Notification.Do("Locked!");
    }

}
