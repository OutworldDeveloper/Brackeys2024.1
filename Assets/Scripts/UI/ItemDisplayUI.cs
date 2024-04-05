using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemDisplayUI : MonoBehaviour
{

    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _label;

    public void Init(ItemStack stack)
    {
        _image.sprite = stack.Item.Sprite;
        _label.text = stack.Item.DisplayName;
    }

}
