using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class UI_InteractionLabel : MonoBehaviour
{

    [SerializeField] private TMP_Text _buttonLabel;
    [SerializeField] private TMP_Text _interactionLabel;

    public void Setup(string button, string text)
    {
        _buttonLabel.text = $"[{button}]";
        _interactionLabel.text = text;
    }

}
