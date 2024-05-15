using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapScreenshotWindow : EditorWindow
{

    [MenuItem("Window/Hallway/Map Screenshot")]
    public static void Open()
    {
        var window = GetWindow<MapScreenshotWindow>("Map Screenshot");
        window.Show();
    }

    private Vector3 CameraPosition
    {
        get => MapScreenshotData.instance.CameraPosition;
        set => MapScreenshotData.instance.CameraPosition = value;
    }

    private int Size
    {
        get => MapScreenshotData.instance.OrthographicSize;
        set => MapScreenshotData.instance.OrthographicSize = value;
    }

    private int ScreenshotSize
    {
        get => MapScreenshotData.instance.ImageSize;
        set => MapScreenshotData.instance.ImageSize = value;
    }

    private Camera _previewCamera;
    private RenderTexture _previewRT;

    private void OnEnable()
    {
        _previewCamera = new GameObject().AddComponent<Camera>();
        _previewCamera.gameObject.AddComponent<UniversalAdditionalCameraData>()
            .renderPostProcessing = false;
        _previewCamera.gameObject.hideFlags = HideFlags.DontSave;
        _previewCamera.orthographic = true;
        _previewCamera.orthographicSize = Size;

        _previewCamera.transform.position = CameraPosition;
        _previewCamera.transform.forward = Vector3.down;

        _previewRT = new RenderTexture(256, 256, 16);
        _previewCamera.targetTexture = _previewRT;

        _previewCamera.Render();
    }

    private void OnDisable()
    {
        _previewCamera.targetTexture = null;
        DestroyImmediate(_previewRT);
        DestroyImmediate(_previewCamera.gameObject);
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        MapScreenshotData.instance.CameraPosition = EditorGUILayout.Vector3Field("Position", MapScreenshotData.instance.CameraPosition);
        MapScreenshotData.instance.OrthographicSize = EditorGUILayout.IntField("Size", MapScreenshotData.instance.OrthographicSize);
        ScreenshotSize = EditorGUILayout.IntField("Screenshot Size", ScreenshotSize);

        if (EditorGUI.EndChangeCheck())
        {
            _previewCamera.orthographicSize = Size;
            _previewCamera.transform.position = CameraPosition;
            _previewCamera.transform.forward = Vector3.down;
            _previewCamera.Render();
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label(_previewRT);
        if (GUILayout.Button("Take Screenshot"))
        {
            TakeScreenshot();
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void TakeScreenshot()
    {
        Camera camera = new GameObject().AddComponent<Camera>();
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>().
            renderPostProcessing = false;
        camera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        camera.orthographic = true;
        camera.orthographicSize = Size;

        camera.transform.position = CameraPosition;
        camera.transform.forward = Vector3.down;

        RenderTexture renderTexture = RenderTexture.GetTemporary(ScreenshotSize, ScreenshotSize, 24, RenderTextureFormat.ARGB32);
        Texture2D finalTexture = new Texture2D(ScreenshotSize, ScreenshotSize, TextureFormat.ARGB32, false);

        RenderTexture previouslyActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        camera.targetTexture = renderTexture;

        camera.Render();

        Rect grabArea = new Rect(0, 0, ScreenshotSize, ScreenshotSize);
        finalTexture.ReadPixels(grabArea, 0, 0);
        finalTexture.Apply();
        byte[] pngShot = finalTexture.EncodeToPNG();

        string path = EditorUtility.SaveFilePanel("Where to save?", string.Empty, $"Map Screenshot from {CameraPosition}.{Size}.{ScreenshotSize}", "png");
        File.WriteAllBytes(path, pngShot);

        RenderTexture.active = previouslyActive;
        RenderTexture.ReleaseTemporary(renderTexture);
        DestroyImmediate(camera.gameObject);
        DestroyImmediate(finalTexture);

        MapScreenshotData.SaveData();
    }

}

[FilePath("Tools/MapScreenshot", FilePathAttribute.Location.ProjectFolder)]
public class MapScreenshotData : ScriptableSingleton<MapScreenshotData>
{

    public Vector3 CameraPosition = new Vector3(0, 100, 0);
    public int OrthographicSize = 35;
    public int ImageSize = 2048;

    public static void SaveData()
    {
        instance.Save(true);
    }

}