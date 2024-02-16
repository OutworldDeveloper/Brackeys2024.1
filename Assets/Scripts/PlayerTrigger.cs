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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player) == true)
        {
            PlayerInside = null;
            ExitEvent.Invoke(player);
        }
    }

}
