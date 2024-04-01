using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableComponents))]
public class StaticSaveable : MonoBehaviour
{

    [SerializeField] private bool _hasSceneGuid;
    [SerializeField] private string _sceneGuid;

    public string SceneGuid => _sceneGuid;

    private void OnValidate()
    {
        //Debug.Log("Update");

        if (gameObject.scene.IsValid() == false)
        {
            Debug.Log("We're a prefab");
            _hasSceneGuid = false;
            _sceneGuid = null;
            return;
        }

        //Debug.Log("We're not a prefab");

        if (_hasSceneGuid == true)
            return;

        //Debug.Log("String is empty");

        _sceneGuid = Guid.NewGuid().ToString();
        _hasSceneGuid = true;
    }

}

public interface IFirstLoadCallback
{
    public void OnFirstLoad();

}
