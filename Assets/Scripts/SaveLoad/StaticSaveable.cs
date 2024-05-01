using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SaveableComponents))]
public class StaticSaveable : MonoBehaviour
{

    [SerializeField] private string _sceneGuid;

    public string SceneGuid => _sceneGuid;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying == true)
            return;

        if (gameObject.scene.IsValid() == false)
        {
            _sceneGuid = null;
            return;
        }

        _sceneGuid = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
    }
#endif

}

public interface IFirstLoadCallback
{
    public void OnFirstLoad();

}
