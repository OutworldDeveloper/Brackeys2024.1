using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    [SerializeField] private Sound _shootSound;
    [SerializeField] private AudioSource _shootSource;

    [SerializeField] private LayerMask _shootMask;

    private TimeSince _timeSinceLastShoot;

    public void Attack(Vector3 origin, Vector3 direction)
    {
        if (_timeSinceLastShoot < 0.2f)
            return;

        _timeSinceLastShoot = TimeSince.Now();

        _shootSound.Play(_shootSource);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 25f, _shootMask) == false)
            return;

        if (hit.transform.TryGetComponent(out Ghost ghost) == true)
        {
            ghost.StartRespawning();
        }
    }

}
