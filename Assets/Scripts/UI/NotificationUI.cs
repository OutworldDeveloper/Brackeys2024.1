using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public sealed class NotificationUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _label;
    private TimeUntil _timeUntilHidden;
    private bool _isNotificationShown;
    private Sequence _currentSequence;

    private void OnEnable()
    {
        _label.gameObject.SetActive(false);
        Notification.Event += OnNotificationSent;
    }

    private void OnDisable()
    {
        Notification.Event -= OnNotificationSent;
    }

    private void OnNotificationSent(Notification.Info info)
    {
        _label.text = info.Text;
        _label.gameObject.SetActive(true);
        _isNotificationShown = true;
        _timeUntilHidden = new TimeUntil(Time.time + info.Duration);

        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence();
        _currentSequence.
            Join(_label.DOFade(1f, 0.4f).From(0f)).
            Join(_label.rectTransform.
                DOLocalMoveY(_label.rectTransform.localPosition.y, 0.2f).
                From(_label.rectTransform.localPosition.y - 25f));
    }

    private void Update()
    {
        if (_isNotificationShown == false)
            return;

        if (_timeUntilHidden < 0f)
        {
            _currentSequence?.Kill();

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_label.DOFade(0f, 0.2f));

            //_label.gameObject.SetActive(false);
            _isNotificationShown = false;
        }
    }

}

public static class Notification
{

    public static event Action<Info> Event;

    public static void Show(string text, float duration = 1f)
    {
        Event?.Invoke(new Info(text, duration));
    }

    public readonly struct Info
    {

        public readonly string Text;
        public readonly float Duration;

        public Info(string text, float duration)
        {
            Text = text;
            Duration = duration;
        }

    }

}
