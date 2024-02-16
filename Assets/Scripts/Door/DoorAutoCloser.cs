using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoCloser : MonoBehaviour
{

    private const bool doCloseDoors = false;

    [SerializeField] private Door _door;
    [SerializeField] private float _closeDelay = 2f;

    private TimeSince _timeSinceLastOpening;

    private void OnEnable()
    {
        _door.Opened += OnDoorOpened;

        if (doCloseDoors == false)
            enabled = false;
    }

    private void OnDisable()
    {
        _door.Opened -= OnDoorOpened;
    }

    private void Update()
    {
        if (_door.IsOpen == false)
            return;

        if (_timeSinceLastOpening > _closeDelay)
        {
            _door.Close();
        }
    }

    private void OnDoorOpened()
    {
        _timeSinceLastOpening = new TimeSince(Time.time);
    }


}
