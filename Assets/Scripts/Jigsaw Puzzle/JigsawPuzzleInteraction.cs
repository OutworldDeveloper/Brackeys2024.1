using UnityEngine;

public sealed class JigsawPuzzleInteraction : Interaction
{

    [SerializeField] private JigsawPuzzle _jigsawPuzzle;

    public override string Text => "Inspect";

    public override void Perform(PlayerCharacter player)
    {
        player.Player.PawnStack.Push(_jigsawPuzzle);
    }

}
