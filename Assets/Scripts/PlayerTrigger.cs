using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerTrigger : MonoBehaviour
{

    [System.Serializable]
    public sealed class PlayerEvent : UnityEvent<PlayerCharacter> { }

    [field: SerializeField] public PlayerEvent EnterEvent { get; private set; }
    [field: SerializeField] public PlayerEvent ExitEvent { get; private set; }

    [field: SerializeField] private PlayerEvent _stayEvent { get; set; }
    [SerializeField] private bool _useStayEvent;
    [SerializeField] private float _stayTimeRequired = 0.75f;

    private bool _stayEventSent;
    private TimeSince _timeSinceLastEntered = new TimeSince(float.NegativeInfinity);

    public bool EverVisited { get; private set; }
    public bool HasPlayerInside => PlayerInside != null;
    public PlayerCharacter PlayerInside { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player) == true)
        {
            PlayerInside = player;
            EverVisited = true;
            EnterEvent.Invoke(player);

            _timeSinceLastEntered = new TimeSince(Time.time);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player) == true)
        {
            PlayerInside = null;
            ExitEvent.Invoke(player);
            _stayEventSent = false;
        }
    }

    private void Update()
    {
        if (_useStayEvent == false || PlayerInside == false || _stayEventSent == true)
            return;

        if (_timeSinceLastEntered > _stayTimeRequired)
        {
            _stayEvent.Invoke(PlayerInside);
            _stayEventSent = true;
        }
    }

}
