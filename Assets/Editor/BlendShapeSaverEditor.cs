using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BlendShapeSaverEditor : EditorWindow
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private string fileName;

    [MenuItem("Tools/Blend Shape Saver")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeSaverEditor>("Blend Shape Saver");
    }

    private void OnGUI()
    {
        GUILayout.Label("Blend Shape Saver", EditorStyles.boldLabel);

        skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Save Blend Shapes"))
        {
            SaveBlendShapes();
        }
    }

    private void SaveBlendShapes()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is not assigned.");
            return;
        }

        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        int blendShapeCount = mesh.blendShapeCount;
        List<BlendShapeInfo> blendShapeInfos = new List<BlendShapeInfo>();

        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendShapeName = mesh.GetBlendShapeName(i);
            blendShapeInfos.Add(new BlendShapeInfo { Index = i, Name = blendShapeName });
        }

        string json = JsonUtility.ToJson(new BlendShapeList { BlendShapes = blendShapeInfos }, true);
        string path = Path.Combine(Application.dataPath, fileName + ".json");
        File.WriteAllText(path, json);

        Debug.Log("Blend shapes saved to " + path);
        AssetDatabase.Refresh();
    }
}