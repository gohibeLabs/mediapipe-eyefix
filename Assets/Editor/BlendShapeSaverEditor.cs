using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Arnab.Scripts;

public class BlendShapeSaverEditor : EditorWindow
{
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private string _fileName;

    [MenuItem("Tools/Blend Shape Saver")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeSaverEditor>("Blend Shape Saver");
    }

    private void OnGUI()
    {
        GUILayout.Label("Blend Shape Saver", EditorStyles.boldLabel);

        _skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", _skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);
        _fileName = EditorGUILayout.TextField("File Name", _fileName);

        if (GUILayout.Button("Save Blend Shapes"))
            SaveBlendShapes();
    }

    private void SaveBlendShapes()
    {
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(_fileName))
        {
            Debug.LogError("File name is not assigned.");
            return;
        }

        var mesh = _skinnedMeshRenderer.sharedMesh;
        var blendShapeCount = mesh.blendShapeCount;
        var blendShapeInfos = new List<DataStructures.BlendShapeInfo>();

        for (var i = 0; i < blendShapeCount; i++)
        {
            var blendShapeName = mesh.GetBlendShapeName(i);
            blendShapeInfos.Add(new DataStructures.BlendShapeInfo { Index = i, Name = blendShapeName });
        }

        var json = JsonUtility.ToJson(new DataStructures.BlendShapeList { BlendShapes = blendShapeInfos }, true);
        var path = Path.Combine(Application.dataPath, _fileName + ".json");
        File.WriteAllText(path, json);

        Debug.Log("Blend shapes saved to " + path);
        AssetDatabase.Refresh();
    }
}