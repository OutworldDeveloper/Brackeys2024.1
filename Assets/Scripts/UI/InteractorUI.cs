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

    private void OnEnable()
    {
        _player.Interactor.TargetChanged += OnTargetChanged;
    }

    private void OnDisable()
    {
        _player.Interactor.TargetChanged -= OnTargetChanged;
    }

    private void OnTargetChanged(List<Interaction> interactions)
    {
        foreach (var label in _activeLabels)
        {
            Destroy(label.gameObject);
        }

        _activeLabels.Clear();

        for (int i = 0; i < Mathf.Min(interactions.Count, _keyCodes.Length); i++)
        {
            var interaction = interactions[i];
            var text = Instantiate(_interactionLabelPrefab);
            text.transform.SetParent(_interactionsPanel, false);
            _activeLabels.Add(text);
            text.Setup(_keyCodes[i].ToString(), interaction.Text);
        }
    }

}
