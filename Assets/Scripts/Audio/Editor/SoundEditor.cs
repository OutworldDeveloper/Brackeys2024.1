using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sound))]
[CanEditMultipleObjects]
public sealed class SoundEditor : Editor
{

    [SerializeField] private AudioSource _previewSource;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Play Preview") == true)
        {
            _previewSource.transform.position = FindObjectOfType<AudioListener>().transform.position;
            (target as Sound).Play(_previewSource);
        }
    }

    private void OnEnable()
    {
        _previewSource = new GameObject().AddComponent<AudioSource>();
        _previewSource.gameObject.hideFlags = HideFlags.DontSave;
    }

    private void OnDisable()
    {
        DestroyImmediate(_previewSource.gameObject);
    }

}
