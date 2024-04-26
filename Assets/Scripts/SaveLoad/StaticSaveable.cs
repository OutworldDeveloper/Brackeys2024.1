using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableComponents))]
public class StaticSaveable : MonoBehaviour
{

    [SerializeField] private string _sceneGuid;

    public string SceneGuid => _sceneGuid;

    private void OnValidate()
    {
        //Debug.Log("Update");

        if (gameObject.scene.IsValid() == false)
        {
            //Debug.Log("We're a prefab");
            _sceneGuid = null;
            return;
        }

#if UNITY_EDITOR
        _sceneGuid = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
#endif
    }

}

public interface IFirstLoadCallback
{
    public void OnFirstLoad();

}
