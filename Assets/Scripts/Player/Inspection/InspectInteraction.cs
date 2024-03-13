using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public sealed class InspectInteraction : Interaction
{

    private Item _inspectable;

    public override string Text => "Inspect";

    private void Awake()
    {
        _inspectable = GetComponent<Item>();
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inspect(_inspectable);
    }

}
