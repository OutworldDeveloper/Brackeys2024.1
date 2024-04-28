using UnityEngine;

public sealed class ColorLockInteraction : Interaction
{

    [SerializeField] private ColorLockPuzzle _puzzle;

    public override string Text => "Open";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return true;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Player.Possess(_puzzle);
    }

}
