using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(VirtualCamera))]
public class VirtualCameraEditor : Editor
{

    private const string _previewTitle = "Virtual Camera preview";

    private Camera _previewCamera;
    private RenderTexture _previewRT;

    private DrawRenderTextureOverlay _previewOverlay;

    private void OnEnable()
    {
        _previewCamera = new GameObject().AddComponent<Camera>();
        _previewCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        _previewCamera.gameObject.AddComponent<UniversalAdditionalCameraData>()
            .renderPostProcessing = true;
        _previewRT = new RenderTexture(400, 225, 16);
        _previewRT.Create();

        EditorApplication.update += OnEditorUpdate;

        _previewOverlay = new DrawRenderTextureOverlay(_previewRT);
        _previewOverlay.displayName = _previewTitle;
        _previewOverlay.displayed = true;
        SceneView.AddOverlayToActiveView(_previewOverlay);
    }

    private void OnDisable()
    {
        DestroyImmediate(_previewCamera.gameObject);
        _previewRT.Release();

        EditorApplication.update -= OnEditorUpdate;

        SceneView.RemoveOverlayFromActiveView(_previewOverlay);
    }

    private void OnEditorUpdate()
    {
        RedrawPreviewCamera();
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent(_previewTitle);
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        base.OnPreviewGUI(r, background);

        float targetHeight = r.width / 16f * 9f;
        r.height = targetHeight;

        GUI.DrawTexture(r, _previewRT);
    }

    private void RedrawPreviewCamera()
    {
        VirtualCamera virtualCamera = target as VirtualCamera;

        _previewCamera.fieldOfView = virtualCamera.FieldOfView;
        _previewCamera.transform.position = virtualCamera.transform.position;
        _previewCamera.transform.rotation = virtualCamera.transform.rotation;

        _previewCamera.targetTexture = _previewRT;
        _previewCamera.Render();
    }

}

public sealed class DrawRenderTextureOverlay : IMGUIOverlay
{

    private readonly RenderTexture _rt;

    public DrawRenderTextureOverlay(RenderTexture rt)
    {
        _rt = rt;
    }

    public override void OnGUI()
    {
        GUILayout.Label(_rt);
    }

}
