using UnityEngine;
using UnityEngine.Rendering;

public sealed class ScreenFade : MonoBehaviour
{

    public const float FadeSpeed = 1.7f;

    private static int _fadedTimes = 0;
    private static float _fadeInTime = float.NegativeInfinity;

    public static bool IsFadedOut => _fadedTimes > 0 || Time.time < _fadeInTime;

    public static void FadeOutFor(float duration)
    {
        float fadeInTime = Time.time + duration;
        if (fadeInTime > _fadeInTime)
            _fadeInTime = fadeInTime;
    }

    public static void FadeOut()
    {
        _fadedTimes++;
    }

    public static void FadeIn()
    {
        _fadedTimes--;
    }

    [SerializeField] private Volume _fadeVolume;

    private void Start()
    {
        _fadeVolume.weight = IsFadedOut ? 0f : 1f;
    }

    private void Update()
    {
        if (IsFadedOut == true)
        {
            if (_fadeVolume.weight < 1f)
                _fadeVolume.weight += Time.deltaTime * FadeSpeed;
        }
        else
        {
            if (_fadeVolume.weight > 0f)
                _fadeVolume.weight -= Time.deltaTime * FadeSpeed;
        }
    }

}
