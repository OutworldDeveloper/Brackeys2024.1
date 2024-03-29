using TMPro;
using UnityEngine;

public sealed class UI_SaveSlot : MonoBehaviour
{

    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private TextMeshProUGUI _savesCountLabel;
    [SerializeField] private TextMeshProUGUI _difficultyLabel;
    [SerializeField] private TextMeshProUGUI _saveDateLabel;
    [SerializeField] private TextMeshProUGUI _playTimeLabel;

    public void Display(UI_SaveSlotInfo info)
    {
        _infoPanel.SetActive(true);

        _savesCountLabel.text = info.SavedTimes.ToString();
        _saveDateLabel.text = info.LastSavedTime.ToString();
        _playTimeLabel.text = info.PlayTime.ToString();
    }

    public void DisplayEmpty()
    {
        _infoPanel.SetActive(false);
    }

}
