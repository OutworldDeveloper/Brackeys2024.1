using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerTeleporter : MonoBehaviour
{

    [SerializeField] private Vector3 _targetPosition;
   
    public void Teleport(PlayerCharacter player)
    {
        player.Warp(_targetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(_targetPosition + Vector3.up * 0.5f, 0.5f);
    }

}
