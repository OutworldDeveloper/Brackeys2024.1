using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inspectable))]
public sealed class InspectInteraction : Interaction
{

    private Inspectable _inspectable;

    public override string Text => "Inspect";

    private void Awake()
    {
        _inspectable = GetComponent<Inspectable>();
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inspect(_inspectable);
    }

}
