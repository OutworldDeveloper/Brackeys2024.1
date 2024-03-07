using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInteraction : MonoBehaviour
{

    public event Action TargetChanged;

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _minInteractionRange = 0.75f;
    [SerializeField] private float _interactionRange = 2.5f;

    [SerializeField] private Interaction[] _globalInteractinos;

    private GameObject _currentTarget;
    private readonly List<Interaction> _targetInteractions = new List<Interaction>();
    private bool _hasTarget;

    public int InteractionsCount => _targetInteractions.Count;
    public Interaction GetInteraction(int index) => _targetInteractions[index];
    public bool HasTarget => _hasTarget;
    public GameObject Target => _currentTarget;

    public int GetAvaliableInteractionsCount()
    {
        int avaliableCount = 0;
        for (int i = 0; i < InteractionsCount; i++)
        {
            var interaction = GetInteraction(i);

            if (interaction.IsAvaliable(_player) == true)
            {
                avaliableCount++;
            }
        }

        return avaliableCount;
    }

    public Interaction GetAvaliableInteraction(int index)
    {
        int avaliableIndex = -1;
        for (int i = 0; i < InteractionsCount; i++)
        {
            var interaction = GetInteraction(i);

            if (interaction.IsAvaliable(_player) == true)
            {
                avaliableIndex++;

                if (avaliableIndex == index)
                {
                    return interaction;
                }
            }
        }

        return null;
    }

    public void TryPerform(int index)
    {
        if (_targetInteractions.Count <= index)
            return;

        int avaliableIndex = -1;
        for (int i = 0; i < InteractionsCount; i++)
        {
            var interaction = GetInteraction(i);

            if (interaction.IsAvaliable(_player) == true)
            {
                avaliableIndex++;

                if (avaliableIndex == index)
                {
                    interaction.Perform(_player);
                    return;
                }
            }
        }
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * GetInteractionRange(ray), Color.blue);

        if (Physics.Raycast(ray, out RaycastHit hit, GetInteractionRange(ray), _interactableLayer))
        {
            if (hit.transform.gameObject != _currentTarget)
            {
                OnTargetChanged(hit.transform.gameObject);
                _hasTarget = true;
            }
        }
        else
        {
            if (_hasTarget == true)
            {
                OnTargetChanged(null);
                _hasTarget = false;
            }
        }
    }

    private float GetInteractionRange(Ray ray)
    {
        float dot = Vector3.Dot(ray.direction, Vector3.down);
        float t = (dot + 1) / 2;
        return Mathf.Lerp(_minInteractionRange, _interactionRange, t);
    }

    private void OnTargetChanged(GameObject target)
    {
        _targetInteractions.Clear();
        _currentTarget = target;

        foreach (var interaction in _globalInteractinos)
        {
            _targetInteractions.Add(interaction);
        }

        if (target != null)
        {
            GetInteractionsForTarget(_currentTarget, _targetInteractions);
        }

        TargetChanged?.Invoke();
    }

    private void GetInteractionsForTarget(GameObject target, List<Interaction> interactions)
    {
        foreach (var interaction in target.GetComponents<Interaction>())
        {
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
