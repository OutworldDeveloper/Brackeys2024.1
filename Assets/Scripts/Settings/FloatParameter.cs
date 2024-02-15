using UnityEngine;

[CreateAssetMenu(fileName = "New Float Parameter", menuName = "Settings/Float")]
public class FloatParameter : Parameter<float>
{

    [field: SerializeField] public float MinValue { get; private set; }
    [field: SerializeField] public float MaxValue { get; private set; }

    protected override float LoadValue()
    {
        return PlayerPrefs.GetFloat(name);
    }

    protected override void SaveValue(float value)
    {
        PlayerPrefs.SetFloat(name, value);
    }

}