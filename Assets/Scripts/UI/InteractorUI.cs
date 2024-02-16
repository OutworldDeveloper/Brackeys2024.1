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

    private readonly List<UI_InteractionLabel> _activeLabels = new List<UI_InteractionLabel>();

    private List<Interaction> _interactions;
    private TimeSince _timeSinceLastRefresh;

    private void OnEnable()
    {
        _player.Interactor.TargetChanged += OnTargetChanged;
    }

    private void OnDisable()
    {
        _player.Interactor.TargetChanged -= OnTargetChanged;
        Clear();
        _interactions = null;
    }

    private void Update()
    {
        // What the fuck!
        if (_timeSinceLastRefresh > 0.25f && _interactions != null)
        {
            _timeSinceLastRefresh = new TimeSince(Time.time);

            for (int i = 0; i < Mathf.Min(_interactions.Count, _keyCodes.Length); i++)
            {
                var text = _activeLabels[i];
                var interaction = _interactions[i];
                text.Setup(_keyCodes[i].ToString(), interaction.Text);
            }
        }
    }

    private void OnTargetChanged(List<Interaction> interactions)
    {
        _interactions = interactions;
        _timeSinceLastRefresh = new TimeSince(Time.time);

        Clear();

        for (int i = 0; i < Mathf.Min(interactions.Count, _keyCodes.Length); i++)
        {
            var interaction = interactions[i];
            var text = Instantiate(_interactionLabelPrefab);
            text.transform.SetParent(_interactionsPanel, false);
            _activeLabels.Add(text);
            text.Setup(_keyCodes[i].ToString(), interaction.Text);
        }
    }

    private void Clear()
    {
        foreach (var label in _activeLabels)
        {
            Destroy(label.gameObject);
        }

        _activeLabels.Clear();
    }

}
