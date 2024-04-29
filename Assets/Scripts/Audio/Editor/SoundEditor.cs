using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sound))]
[CanEditMultipleObjects]
public sealed class SoundEditor : Editor
{

    private static AudioSource _previewSource;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Play Preview") == true)
        {
            PlayPreview();
        }
    }

    private void OnDisable()
    {
        if (_previewSource == null)
            return;

        DestroyImmediate(_previewSource.gameObject);
    }

    private void PlayPreview()
    {
        EnsurePreviewSource();
        _previewSource.transform.position = FindObjectOfType<AudioListener>().transform.position;
        (target as Sound).Play(_previewSource);
    }

    private void EnsurePreviewSource()
    {
        if (_previewSource != null)
            return;

        _previewSource = new GameObject().AddComponent<AudioSource>();
        _previewSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

}
