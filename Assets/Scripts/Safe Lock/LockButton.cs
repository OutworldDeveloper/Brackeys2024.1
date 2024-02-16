using UnityEngine;
using DG.Tweening;

public sealed class LockButton : MonoBehaviour
{

    private Tween _currentTween;
    private float _originalPositionZ;
    private float _selectedPositionZ;
    private float _pressedPositionZ;

    private void Start()
    {
        _originalPositionZ = transform.localPosition.z - 0.01f;
        _selectedPositionZ = _originalPositionZ + 0.01f;
        _pressedPositionZ = _originalPositionZ - 0.005f;

        transform.localPosition = new Vector3()
        {
            x = transform.localPosition.x,
            y = transform.localPosition.y,
            z = _originalPositionZ,
        };
    }

    public void OnSelected()
    {
        _currentTween?.Kill();
        _currentTween = transform.DOLocalMoveZ(_selectedPositionZ, 0.05f);
    }

    public void OnDeselected()
    {
        _currentTween?.Kill();
        _currentTween = transform.DOLocalMoveZ(_originalPositionZ, 0.05f);
    }

    public void OnPressed()
    {
        _currentTween?.Kill();
        _currentTween = transform.DOLocalMoveZ(_pressedPositionZ, 0.2f).
            SetLoops(2, LoopType.Yoyo);
    }

}
