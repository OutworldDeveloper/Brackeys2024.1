using UnityEngine;

[System.Serializable]
public struct Prefab<T> where T : Component
{

    [SerializeField] private T _asset;

    public T Instantiate()
    {
        var instance = Object.Instantiate(_asset);
        instance.name = _asset.name;
        return instance;
    }

    public T Instantiate(Vector3 position, Vector3 facingDirection)
    {
        var instance = Instantiate();
        instance.transform.position = position;
        instance.transform.forward = facingDirection;
        return instance;
    }

    public T Instantiate(Vector3 position, Quaternion rotation)
    {
        var instance = Instantiate();
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

}