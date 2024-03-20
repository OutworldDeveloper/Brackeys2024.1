using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    [SerializeField] private float _cooldown = 0.2f;
    
    [SerializeField] private Sound _shootSound;
    [SerializeField] private AudioSource _shootSource;

    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private float _muzzleFlashDuration = 0.035f;

    [SerializeField] private LayerMask _shootMask;

    private TimeSince _timeSinceLastShoot;

    public bool CanAim => true;

    public void Attack(Vector3 origin, Vector3 direction)
    {
        if (_timeSinceLastShoot < _cooldown)
            return;

        _timeSinceLastShoot = TimeSince.Now();

        _shootSound.Play(_shootSource);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 25f, _shootMask) == false)
            return;

        if (hit.transform.TryGetComponent(out Zombie zombie) == true)
        {
            zombie.Kill();
        }
    }

    private void Update()
    {
        _muzzleFlash.SetActive(_timeSinceLastShoot < _muzzleFlashDuration);
    }

}
