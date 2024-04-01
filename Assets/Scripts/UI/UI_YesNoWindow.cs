using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Order.UI)]
public class UI_YesNoWindow : UI_Panel
{

    [SerializeField] private TextMeshProUGUI _questionLabel;
    [SerializeField] private TextMeshProUGUI _descriptionLabel;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private Action _confirmAction;
    private Action _cancelAction;

    private void Awake()
    {
        _confirmButton.onClick.AddListener(OnConfirmed);
        _cancelButton.onClick.AddListener(OnCanceled);
    }

    public UI_YesNoWindow Setup(string question, string description, Action onConfirm, bool cancelButton = true)
    {
        _questionLabel.text = question;
        _descriptionLabel.text = description;
        _confirmAction = onConfirm;

        _cancelButton.gameObject.SetActive(cancelButton);

        return this;
    }

    private void OnConfirmed()
    {
        _confirmAction?.Invoke();
        CloseAndDestroy();
    }

    private void OnCanceled()
    {
        _cancelAction?.Invoke();
        CloseAndDestroy();
    }

    public override bool CanUserClose()
    {
        return false;
    }

}
