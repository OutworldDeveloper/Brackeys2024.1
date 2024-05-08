using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_PawnActionsGuide : MonoBehaviour
{

    [SerializeField] private Player _player;
    [SerializeField] private Prefab<UI_PawnAction> _displayPrefab;
    [SerializeField] private Transform _parent;

    private readonly List<UI_PawnAction> _currentDisplays = new List<UI_PawnAction>();

    private void Awake()
    {
        _player.PawnStack.ActivePawnChanged += OnPawnChanged;
        _parent.gameObject.SetActive(false);
    }

    private void OnPawnChanged(Pawn pawn)
    {
        foreach (var display in _currentDisplays)
        {
            Destroy(display.gameObject);
        }

        _currentDisplays.Clear();

        if (pawn.HasActions == false)
        {
            _parent.gameObject.SetActive(false);
            return;
        }

        _parent.gameObject.SetActive(true);

        foreach (var action in pawn.GetActions())
        {
            var display = _displayPrefab.Instantiate();
            display.SetTarget(action);
            display.transform.SetParent(_parent, false);

            _currentDisplays.Add(display);
        }
    }

}
