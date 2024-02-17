using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoCloser : MonoBehaviour
{

    private const bool doCloseDoors = true;
    private const float closeDelay = 4f;

    [SerializeField] private Door _door;

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

        if (_timeSinceLastOpening > closeDelay)
        {
            _door.Close();
        }
    }

    private void OnDoorOpened()
    {
        _timeSinceLastOpening = new TimeSince(Time.time);
    }


}
