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
            if (_previewSource == null)
            {
                _previewSource = new GameObject().AddComponent<AudioSource>();
                _previewSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
                _previewSource.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }

            (target as Sound).Play(_previewSource);
        }
    }

    private void OnDisable()
    {
        if (_previewSource == null)
            return;

        DestroyImmediate(_previewSource.gameObject);
    }

}
