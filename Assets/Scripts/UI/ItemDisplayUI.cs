using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemDisplayUI : MonoBehaviour
{

    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _label;

    public void Init(Item item)
    {
        _image.sprite = item.Sprite;
        _label.text = item.DisplayName;
    }

}
