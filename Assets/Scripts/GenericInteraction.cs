using UnityEngine;
using UnityEngine.Events;

public sealed class GenericInteraction : Interaction
{

    [System.Serializable]
    private sealed class PlayerEvent : UnityEvent<PlayerCharacter> { }

    [SerializeField] private string _text;
    [SerializeField] private PlayerEvent _interaction;

    public override string Text => _text;

    public override void Perform(PlayerCharacter player)
    {
        _interaction.Invoke(player);
    }

}
