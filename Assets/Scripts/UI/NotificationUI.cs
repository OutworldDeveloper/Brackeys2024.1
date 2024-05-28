using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public sealed class NotificationUI : MonoBehaviour
{

    [SerializeField] private CanvasGroup _notificationGroup;
    [SerializeField] private Prefab<TextMeshProUGUI> _labelPrefab;
    [SerializeField] private Prefab<UI_KeyHint> _keyHintPrefab;

    [SerializeField] private Color _highlightedColor = Color.yellow;

    private TimeUntil _timeUntilHidden;
    private bool _isNotificationShown;
    private Sequence _currentSequence;

    private void OnEnable()
    {
        _notificationGroup.alpha = 0.0f;
        Notification.Event += OnNotificationSent;
    }

    private void OnDisable()
    {
        Notification.Event -= OnNotificationSent;
    }

    private void Start()
    {
        ClearNotification();
    }

    private void Update()
    {
        if (_isNotificationShown == false)
            return;

        if (_timeUntilHidden < 0f)
        {
            _currentSequence?.Kill();

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_notificationGroup.DOFade(0f, 0.2f));

            _isNotificationShown = false;
        }
    }

    private void OnNotificationSent(Notification notification)
    {
        ClearNotification();
        ConstructNotification(notification);

        _isNotificationShown = true;
        _timeUntilHidden = new TimeUntil(Time.time + notification.Duration);

        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence();
        _currentSequence.Join(_notificationGroup.DOFade(1f, 0.4f).From(0f));
    }

    private void ClearNotification()
    {
        foreach (Transform child in _notificationGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ConstructNotification(Notification notification)
    {
        for (int i = 0; i < notification.SegmentsCount; i++)
        {
            NotificationSegment segment = notification.GetSegment(i);
            ConstructSegment(segment);

            bool addSpacer = (i + 1 < notification.SegmentsCount) ?
                segment.GetType() != notification.GetSegment(i + 1).GetType() : false;

            if (addSpacer == true)
                AddSpacer();
        }
    }

    private void ConstructSegment(NotificationSegment segment)
    {
        switch (segment)
        {
            case TextSegment textSegment: ConstructTextSegment(textSegment); break;
            case KeySegment keySegment: ConstructKeySegment(keySegment); break;
        }
    }

    private void AddSpacer()
    {
        var layoutElement = new GameObject().AddComponent<LayoutElement>();
        layoutElement.minWidth = 5;
        layoutElement.transform.SetParent(_notificationGroup.transform, false);
    }

    private void ConstructTextSegment(TextSegment textSegment)
    {
        var label = _labelPrefab.Instantiate();
        label.text = textSegment.Text;
        label.transform.SetParent(_notificationGroup.transform, false);

        if (textSegment.Style == NotificationStyle.Highlighted)
            label.color = _highlightedColor;
    }

    private void ConstructKeySegment(KeySegment keySegment)
    {
        var keyHint = _keyHintPrefab.Instantiate();
        keyHint.Show(keySegment.Key);
        keyHint.transform.SetParent(_notificationGroup.transform, false);
    }

}

public sealed class Notification
{

    public static event Action<Notification> Event;

    public static void Show(string text, float duration = 1f)
    {
        new Notification(duration).Append(text).Show();
    }

    public static void ShowDebug(string text, float duration = 1f)
    {
        if (Application.isEditor == false)
            return;

        Show($"[Debug] {text}", duration);
    }

    private readonly List<NotificationSegment> _segments = new();

    public Notification(float duration = 1f)
    {
        Duration = duration;
    }

    public float Duration { get; }
    public int SegmentsCount => _segments.Count;

    public NotificationSegment GetSegment(int index)
    {
        return _segments[index];
    }

    public Notification Append(string text, NotificationStyle style = NotificationStyle.Default)
    {
        _segments.Add(new TextSegment(text, style));
        return this;
    }

    public Notification Append(params KeyCode[] keys)
    {
        foreach (var key in keys)
        {
            _segments.Add(new KeySegment(key));
        }

        return this;
    }

    public void Show()
    {
        Event?.Invoke(this);
    }

}

public abstract class NotificationSegment { }

public sealed class TextSegment : NotificationSegment
{
    
    public readonly string Text;
    public readonly NotificationStyle Style;

    public TextSegment(string text, NotificationStyle style = NotificationStyle.Default)
    {
        Text = text;
        Style = style;
    }

}

public enum NotificationStyle
{
    Default,
    Highlighted
}

public sealed class KeySegment : NotificationSegment
{

    public readonly KeyCode Key;

    public KeySegment(KeyCode key)
    {
        Key = key;
    }

}
