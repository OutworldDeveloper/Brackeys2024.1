using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class InteractorUI : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Transform _interactionsPanel;
    [SerializeField] private UI_InteractionLabel _interactionLabelPrefab;
    [SerializeField] private KeyCode[] _keyCodes;

    private UI_InteractionLabel[] _interactionLables;
    private TimeSince _timeSinceLastRefresh;

    private void OnEnable()
    {
        _player.Interactor.TargetChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _player.Interactor.TargetChanged -= Refresh;
    }

    private void Update()
    {
        if (_timeSinceLastRefresh > 0.1f)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        SpawnInteractionLabelsIfNeeded();

        _timeSinceLastRefresh = new TimeSince(Time.time);

        // This keeps button the same I believe?
        /*
        for (int i = 0; i < _interactionLables.Length; i++)
        {
            var label = _interactionLables[i];
            label.gameObject.SetActive(false);

            if (i > _player.Interactor.InteractionsCount - 1)
                continue;

            var interaction = _player.Interactor.GetInteraction(i);

            if (interaction.IsAvaliable(_player) == true)
            {
                label.gameObject.SetActive(true);
                label.SetInteractionText(interaction.Text);
            }
        }
        */

        for (int i = 0; i < _interactionLables.Length; i++)
        {
            var label = _interactionLables[i];
            label.gameObject.SetActive(false);
        }

        for (int i = 0; i < Mathf.Min(_interactionLables.Length, _player.Interactor.GetAvaliableInteractionsCount()); i++)
        {
            var interaction = _player.Interactor.GetAvaliableInteraction(i);
            var label = _interactionLables[i];
            label.gameObject.SetActive(true);
            label.SetInteractionText(interaction.Text);
        }
    }

    private void SpawnInteractionLabelsIfNeeded()
    {
        if (_interactionLables != null)
            return;

        _interactionLables = new UI_InteractionLabel[_keyCodes.Length];

        for (int i = 0; i < _keyCodes.Length; i++)
        {
            var interactionLabel = Instantiate(_interactionLabelPrefab);
            interactionLabel.transform.SetParent(_interactionsPanel, false);
            interactionLabel.SetKeyCode(_keyCodes[i]);
            _interactionLables[i] = interactionLabel;
        }
    }

}
