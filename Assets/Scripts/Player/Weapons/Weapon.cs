using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    [field: SerializeField] public int AnimationSet { get; private set; } 
    
    [SerializeField] private Sound _shootSound;
    [SerializeField] private AudioSource _shootSource;

    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private float _muzzleFlashDuration = 0.035f;

    private TimeSince _timeSinceLastShoot;

    public void OnAttack(Vector3 origin, Vector3 direction)
    {
        _timeSinceLastShoot = TimeSince.Now();
        _shootSound.Play(_shootSource);
    }

    private void Start()
    {
        _muzzleFlash.SetActive(false);
    }

    private void Update()
    {
        _muzzleFlash.SetActive(_timeSinceLastShoot < _muzzleFlashDuration);
    }

}
