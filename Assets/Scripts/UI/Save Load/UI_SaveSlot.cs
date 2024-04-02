using System;
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

        _playTimeLabel.text = DurationToPlayTime(info.PlayTime);
    }

    public void DisplayEmpty()
    {
        _infoPanel.SetActive(false);
    }

    private string DurationToPlayTime(float duration)
    {
        var ss = Convert.ToInt32(duration % 60).ToString("00");
        var mm = (Math.Floor(duration / 60) % 60).ToString("00");
        var hh = Math.Floor(duration / 60 / 60).ToString("00");
        return hh + ":" + mm + ":" + ss;
    }

}
