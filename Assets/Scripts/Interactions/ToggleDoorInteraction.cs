using UnityEngine;

public sealed class ToggleDoorInteraction : Interaction
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

        if (_door.IsLocked == true)
        {
            _door.TryOpen();
            player.Player.OpenItemSelection(new KeySelector(_door));
        }
        else
        {
            _door.TryOpen();
        }
    }

    private sealed class KeySelector : IItemSelector
    {

        private readonly Door _door;

        public KeySelector(Door door)
        {
            _door = door;
        }

        public bool CanAccept(IReadOnlyStack stack)
        {
            return stack.Item is KeyItem;
        }

        public void Select(ItemStack stack)
        {
            _door.TryUnlock(stack);
            _door.TryOpen();
        }

    }

}
