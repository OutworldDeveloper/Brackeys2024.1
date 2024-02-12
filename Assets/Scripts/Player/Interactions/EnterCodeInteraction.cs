﻿using UnityEngine;

public sealed class EnterCodeInteraction : Interaction
{

    [SerializeField] private SafeLock _codeLock;

    public override string Text => "Enter code";

    public override void Perform(PlayerCharacter player)
    {
        player.Player.Possess(_codeLock);
    }

}
