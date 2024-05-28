using UnityEngine;
using TMPro;

public sealed class UI_InteractionLabel : MonoBehaviour
{

    [SerializeField] private UI_KeyHint _keyHint;
    [SerializeField] private TMP_Text _interactionLabel;

    public void SetKeyCode(KeyCode keyCode)
    {
        _keyHint.Show(keyCode);
    }

    public void SetInteractionText(string text)
    {
        _interactionLabel.text = text;
    }

}
