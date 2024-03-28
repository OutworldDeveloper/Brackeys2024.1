using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(SaveableComponents))]
public class DynamicSaveable : MonoBehaviour
{

    [SerializeField] private GameObject _selfPrefab;
    [SerializeField] private string _prefabPath;
    [SerializeField] private bool _hasSceneID;
    [SerializeField] private string _sceneID;

    public string PrefabPath => _prefabPath;
    public string SceneGuid => _sceneID;

    public void SetSceneGuid(string guid)
    {
        _hasSceneID = true;
        _sceneID = guid;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (gameObject.scene == default)
        {
            _hasSceneID = false;
            _sceneID = string.Empty;
        }

        if (Application.isPlaying == true)
            return;

        if (gameObject.scene == default)
        {
            _selfPrefab = PrefabUtility.FindPrefabRoot(gameObject);
        }
        else
        {
            _selfPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (_hasSceneID == false)
            {
                _sceneID = Guid.NewGuid().ToString();
                _hasSceneID = true;
            }
        }

        _prefabPath = AssetDatabase.GetAssetPath(_selfPrefab);
    }
#endif

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        if (_hasSceneID == true)
            return;

        _sceneID = Guid.NewGuid().ToString();
        _hasSceneID = true;
    }

}
