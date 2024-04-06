using UnityEngine;

public sealed class ExpItemConsumer : Interaction, IItemSelector
{

    [SerializeField] private Prefab<UI_InventorySelectScreen> _selectScreen;

    public override string Text => "Select Item";

    public override void Perform(PlayerCharacter player)
    {
        var screen = player.Player.Panels.InstantiateAndOpenFrom(_selectScreen);
        screen.SetTarget(player);
        screen.SetSelector(this);
    }

    public bool CanAccept(IReadOnlyStack stack)
    {
        return true;
    }

    public void Select(ItemStack stack)
    {
        stack.Take(1);
    }

}
