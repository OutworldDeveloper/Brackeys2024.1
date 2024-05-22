using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_KeyHint : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _label;

    public void Show(KeyCode key)
    {
        _label.text = GetKeyLabel(key);
    }

    private string GetKeyLabel(KeyCode key) => key switch
    {
        KeyCode.Return => "Enter",
        KeyCode.Escape => "Esc",
        KeyCode.Alpha0 => "0",
        KeyCode.Alpha1 => "1",
        KeyCode.Alpha2 => "2",
        KeyCode.Alpha3 => "3",
        KeyCode.Alpha4 => "4",
        KeyCode.Alpha5 => "5",
        KeyCode.Alpha6 => "6",
        KeyCode.Alpha7 => "7",
        KeyCode.Alpha8 => "8",
        KeyCode.Alpha9 => "9",
        KeyCode.LeftWindows => "Win",
        KeyCode.RightWindows => "RightWin",
        _ => key.ToString()
    };

}
