using UnityEngine;

[System.Serializable]
public struct Prefab<T> where T : Object
{

    [SerializeField] private T _asset;

    public T Instantiate()
    {
        var instance = Object.Instantiate(_asset);
        instance.name = _asset.name;
        return instance;
    }

}