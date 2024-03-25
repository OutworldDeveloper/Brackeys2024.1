using UnityEngine;

public sealed class TestSaveableComponent : MonoBehaviour
{

    [SerializeField, PersistentAttribute] private float _health = 4f;
    [SerializeField, PersistentAttribute] private int _level = 7;
    [SerializeField, PersistentAttribute] private string _nickname = "aboba";

}
