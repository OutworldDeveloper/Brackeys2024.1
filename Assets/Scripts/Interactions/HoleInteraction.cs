using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HoleInteraction : Interaction
{

    [SerializeField] private Hole _hole;

    public override string Text => "Extract item";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _hole.IsItemExtracted == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _hole.TryExtractItem(player);
    }

}
