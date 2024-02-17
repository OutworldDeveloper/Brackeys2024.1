using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(ScreenshotTaker))]
public sealed class ScreenshotTakerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Go!") == true)
        {
            var screenshotTaker = (target as ScreenshotTaker);

            for (int i = 0; i < screenshotTaker.Items.Length; i++)
            {
                var targetItem = screenshotTaker.Items[i];

                foreach (var item in (target as ScreenshotTaker).Items)
                {
                    item.SetActive(targetItem == item);
                }

                var camera = FindObjectOfType<Camera>();
                var filePath = EditorUtility.SaveFilePanel("Where to save?", string.Empty, targetItem.name, "png");

                TakeTransparentScreenshot(camera, 512, 512, filePath);
                AssetDatabase.Refresh();
            }
        }
    }

    public static void TakeTransparentScreenshot(Camera cam, int width, int height, string savePath)
    {
        var urpCameraData = cam.GetComponent<UniversalAdditionalCameraData>();

        var wasPostprocessing = urpCameraData.renderPostProcessing;
        var wasTargetTexture = cam.targetTexture;
        var wasClearFlags = cam.clearFlags;
        var wasActive = RenderTexture.active;

        var transparentTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var postprocessTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grabArea = new Rect(0, 0, width, height);

        RenderTexture.active = renderTexture;
        cam.targetTexture = renderTexture;
        cam.clearFlags = CameraClearFlags.SolidColor;

        cam.backgroundColor = Color.clear;
        urpCameraData.renderPostProcessing = false;
        cam.Render();
        transparentTexture.ReadPixels(grabArea, 0, 0);
        transparentTexture.Apply();

        urpCameraData.renderPostProcessing = true;
        cam.Render();
        postprocessTexture.ReadPixels(grabArea, 0, 0);
        postprocessTexture.Apply();

        var transparentPixels = transparentTexture.GetPixels();
        var postprocessPixels = transparentTexture.GetPixels();

        for (var i = 0; i < transparentPixels.Length; i++)
        {
            //postprocessPixels[i] = postprocessPixels[i].SetAlpha(transparentPixels[i].a);
        }

        postprocessTexture.SetPixels(postprocessPixels);

        byte[] pngShot = postprocessTexture.EncodeToPNG();
        File.WriteAllBytes(savePath, pngShot);

        cam.clearFlags = wasClearFlags;
        cam.targetTexture = wasTargetTexture;
        urpCameraData.renderPostProcessing = wasPostprocessing;
        RenderTexture.active = wasActive;
        RenderTexture.ReleaseTemporary(renderTexture);
        DestroyImmediate(transparentTexture);
        DestroyImmediate(postprocessTexture);
    }

}
