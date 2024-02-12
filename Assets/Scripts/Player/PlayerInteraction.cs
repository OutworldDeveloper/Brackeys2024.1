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

    private GameObject _currentTarget;
    private readonly List<Interaction> _avaliableInteractions = new List<Interaction>();

    public void TryPerform(int index)
    {
        if (_avaliableInteractions.Count <= index)
            return;

        _avaliableInteractions[index].Perform(_player);
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f, _interactableLayer))
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
            if (interaction.IsAvaliable == false)
                return;

            interactions.Add(interaction);
        }
    }

}

public abstract class Interaction : MonoBehaviour
{
    public abstract string Text { get; }
    public virtual bool IsAvaliable => true;
    public abstract void Perform(PlayerCharacter player);

}
