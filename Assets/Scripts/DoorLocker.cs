using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DoorLocker : MonoBehaviour
{

    private void OnEnable()
    {
        GetComponent<Door>().Block();
    }

    private void OnDisable()
    {
        GetComponent<Door>().Unblock();
    }

}
