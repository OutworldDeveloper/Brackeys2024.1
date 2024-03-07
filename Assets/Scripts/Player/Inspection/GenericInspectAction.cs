using UnityEngine;

public sealed class GenericInspectAction : InspectAction
{

    [SerializeField] private string _interactionText;
    [SerializeField] private string _notificationText;
    [SerializeField] private float _notificationDuration = 1f;

    public override string GetText(PlayerCharacter player)
    {
        return _interactionText;
    }

    public override void Perform(PlayerCharacter player)
    {
        Notification.Show(_notificationText, _notificationDuration);
    }

}
