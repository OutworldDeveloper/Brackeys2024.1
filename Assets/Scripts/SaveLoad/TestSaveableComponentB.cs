using UnityEngine;

public sealed class TestSaveableComponentB : MonoBehaviour
{

    [SerializeField, Persistent] private float _spellPower = 4f;
    [SerializeField, Persistent] private BananaSize _bananaSize;
    [SerializeField, Persistent] private Vector3 _whateverPosition;

    private void Start()
    {
        Notification.ShowDebug($"Your banana size is... {_bananaSize.Size}.", 5f);
    }

}

[System.Serializable]
public struct BananaSize
{
    public string Size;
}
