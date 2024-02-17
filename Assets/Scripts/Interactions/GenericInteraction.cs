using UnityEngine;
using UnityEngine.Events;

public sealed class GenericInteraction : Interaction
{

    [System.Serializable]
    private sealed class PlayerEvent : UnityEvent<PlayerCharacter> { }

    [SerializeField] private string _text;
    [SerializeField] private bool _singleUse;
    [SerializeField] private PlayerEvent _interaction;
    [SerializeField] private bool _sendNotification;
    [SerializeField] private string _notificationText;

    private bool _usedOnce;

    public override string Text => _text;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        if (_usedOnce == true && _singleUse == true)
            return false;

        return base.IsAvaliable(player);
    }

    public override void Perform(PlayerCharacter player)
    {
        _usedOnce = true;

        if (_sendNotification == true)
            Notification.Show(_notificationText);

        _interaction.Invoke(player);
    }

}
