using DG.Tweening;
using UnityEngine;

public sealed class ColorLockRotator : MonoBehaviour
{
    public int CurrentDigit { get; private set; }

    public void RotateUp()
    {
        CurrentDigit--;

        if (CurrentDigit < 0)
            CurrentDigit = 8;

        OnDigitUpdated(true);
    }

    public void RotateDown()
    {
        CurrentDigit++;

        if (CurrentDigit > 8)
            CurrentDigit = 0;

        OnDigitUpdated(true);
    }

    public void SetDigit(int digit)
    {
        CurrentDigit = digit;
        OnDigitUpdated(false);
    }

    private void OnDigitUpdated(bool animate)
    {
        Vector3 targetEuler = new Vector3(0f, 0f, CurrentDigit * 40f);

        if (animate == true)
            transform.DOLocalRotate(targetEuler, 0.2f);
        else
            transform.localEulerAngles = targetEuler;
    }

}
