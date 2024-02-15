using UnityEngine;

public sealed class FridgeChainsInteraction : Interaction
{

    [SerializeField] private FridgeChains _target;

    public override string Text => "Unlock";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _target.IsUnlocked == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _target.TryUnlock(player);
    }
}
