using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerTrigger : MonoBehaviour
{

    [System.Serializable]
    private sealed class PlayerEvent : UnityEvent<PlayerCharacter> { }

    [SerializeField] private PlayerEvent _enterEvent;
    [SerializeField] private PlayerEvent _exitEvent;

    public bool HasPlayerInside => PlayerInside != null;
    public PlayerCharacter PlayerInside { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player) == true)
        {
            PlayerInside = player;
            _enterEvent.Invoke(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player) == true)
        {
            PlayerInside = null;
            _exitEvent.Invoke(player);
        }
    }

}
