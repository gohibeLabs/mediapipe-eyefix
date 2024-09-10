using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;

public class EyeShapeManager : MonoBehaviour
{
    [SerializeField] GameObject template;
    [SerializeField] Transform parentObject;
    public static EyeShapeManager Instance;
    public TextAsset[] jsonFiles;
    [SerializeField] SkinnedMeshRenderer faceMesh, eyelashMesh;
    public string activeEyeShape = "Tsundere_Female";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        { Destroy(gameObject); }
    }

    string[] eyeNames = new string[]
        {
          "Tsundere_Female","Tsundere_Male","Kudere_Female","Kudere_Male","Deredere_Female","Deredere_Male", "Dandere_Female","Dandere_Male", "EyeTriangle", "EyeTriangle_2", "EyeRound_01", "EyeRound_01_2", "EyeRound_02", "EyeRound_02_2", "EyeUpturned", "EyeUpturned_2", "EyeDownturned", "EyeDownturned_2", "EyeAlmond", "EyeAlmond_2", "EyeAlmond_02", "EyeAlmond_02_2", "EyeHonkai", "EyeHonkai_2", "EyePokemon", "EyePokemon_2", "EyeSakura", "EyeSakura_2"
        };


    private void Start()
    {
        foreach (string s in eyeNames)
        {
            GameObject newButton = Instantiate(template, parentObject);
            newButton.SetActive(true);
            newButton.name = s;
            newButton.transform.GetChild(0).GetComponent<Text>().text = s;
        }
    }

    public void SetEyeShape(string eyeShapeName)
    {
        foreach(TextAsset ta in jsonFiles)
        {
            if(ta.name==eyeShapeName)
            {
                FaceShapes fs = JsonUtility.FromJson<FaceShapes>(ta.text);

                foreach(Face face in fs.face)
                {
                    int index;
                    ExpressionApplier.Instance.faceDictionaryAll.TryGetValue(face.name, out index);
                    faceMesh.SetBlendShapeWeight(index, face.value);
                }
                foreach (EyeLash eyeLash in fs.eyeLash)
                {
                    int index;
                    ExpressionApplier.Instance.eyelashDictionaryAll.TryGetValue(eyeLash.name, out index);
                    eyelashMesh.SetBlendShapeWeight(index, eyeLash.value);
                }
                activeEyeShape = eyeShapeName;
                break;
            }
        }
    }
}

[Serializable]
public class Eye
{
    public string name;
    public float value;
}

[Serializable]
public class EyeLash
{
    public string name;
    public float value;
}
[Serializable]
public class Face
{
    public string name;
    public float value;
}
[Serializable]
public class FaceShapes
{
    public List<Face> face;
    public List<object> eyebrow;
    public List<EyeLash> eyeLash;
    public List<Eye> eye;
}