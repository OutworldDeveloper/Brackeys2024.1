using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInteraction : MonoBehaviour
{

    public event Action<List<Interaction>> TargetChanged;

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _interactionRange = 2.5f;

    private GameObject _currentTarget;
    private readonly List<Interaction> _avaliableInteractions = new List<Interaction>();

    public void TryPerform(int index)
    {
        if (_avaliableInteractions.Count <= index)
            return;

        if (_avaliableInteractions[index].IsAvaliable(_player) == false)
            return;

        _avaliableInteractions[index].Perform(_player);
        OnTargetChanged(_currentTarget);
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactableLayer))
        {
            if (hit.transform.gameObject != _currentTarget)
            {
                OnTargetChanged(hit.transform.gameObject);
            }
        }
        else
        {
            if (_currentTarget != null)
            {
                OnTargetChanged(null);
            }
        }
    }

    private void OnTargetChanged(GameObject target)
    {
        _avaliableInteractions.Clear();
        _currentTarget = target;

        if (target != null)
        {
            GetInteractions(_currentTarget, _avaliableInteractions);
        }

        TargetChanged?.Invoke(_avaliableInteractions);
    }

    private void GetInteractions(GameObject target, List<Interaction> interactions)
    {
        foreach (var interaction in target.GetComponents<Interaction>())
        {
            if (interaction.IsAvaliable(_player) == false)
                continue;

            interactions.Add(interaction);
        }
    }

}

public abstract class Interaction : MonoBehaviour
{
    [field: SerializeField] public float InteractionDistance { get; private set; } = 3f;

    public abstract string Text { get; }
    public virtual bool IsAvaliable(PlayerCharacter player) => true;
    public abstract void Perform(PlayerCharacter player);

}
