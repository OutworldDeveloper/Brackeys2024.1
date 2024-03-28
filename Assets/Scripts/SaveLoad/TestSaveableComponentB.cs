using UnityEngine;

public sealed class TestSaveableComponentB : MonoBehaviour
{

    [SerializeField, Persistent] private float _spellPower = 4f;
    [SerializeField, Persistent] private AfricaDefinition _bananaSize;
    [SerializeField, Persistent] private Fucker3 _fucker;
    [SerializeField, Persistent] private Vector3 _whateverPosition;
    [Persistent] private Ducker3 _ducker;
    [SerializeField, Persistent] private int[] _randomInts;

    [Persistent] private ComplexData _complexData;

    private void Start()
    {
        //Notification.ShowDebug($"Your banana size is... {_bananaSize.Size}.", 5f);
        Notification.ShowDebug($"Ducker is {_ducker.x}|{_ducker.y}|{_ducker.z}", 5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            _ducker = new Ducker3()
            {
                x = 3,
                y = 2,
                z = 2
            };
    }

}

[System.Serializable]
public struct Ducker3
{
    public float x, y, z;
}

[System.Serializable]
public struct Fucker3
{
    public float x, y, z;
}

[System.Serializable]
public struct AfricaDefinition
{
    public string Size;
    public Vector3 BananaOffset;
}

[System.Serializable]
public struct ComplexData
{
    public Vector3 BananaOffset;
    public AfricaDefinition BananaSize;
}