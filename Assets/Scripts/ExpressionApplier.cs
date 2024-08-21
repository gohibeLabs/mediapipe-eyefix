using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
//using UnityMainThreadDispatcher;

public class ExpressionApplier : MonoBehaviour
{
    public static ExpressionApplier Instance;

    [SerializeField] SkinnedMeshRenderer face, eyebrow, eyelash,lowerteeth,upperTeeth;
    [SerializeField] TextAsset faceJson, eyebrowJson, eyelashJson,lowerTeethJson,upperTeethJson, bsKeyJson;
    [SerializeField] GameObject faceObject;

    Dictionary<string, int> faceDictionary = new Dictionary<string, int>();
    Dictionary<string, int> eyelashDictionary = new Dictionary<string, int>();
    Dictionary<string, int> eyebrowDictionary = new Dictionary<string, int>();
    Dictionary<string, int> lowerTeethDictionary = new Dictionary<string, int>();
    Dictionary<string, int> UpperTeethDictionary = new Dictionary<string, int>();

    BSCategorynames bSCategorynames = new BSCategorynames();
    BlendShapeList _face = new BlendShapeList();
    BlendShapeList _eyebrow = new BlendShapeList();
    BlendShapeList _eyelash = new BlendShapeList();
    BlendShapeList _lowerTeeth = new BlendShapeList();
    BlendShapeList _upperTeeth = new BlendShapeList();

    public float rotationSmoothing = 1f; // Adjust this for smoothing

    private Quaternion targetRotation;

    public Animator animator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bSCategorynames = JsonUtility.FromJson<BSCategorynames>(bsKeyJson.ToString());
        _face = JsonUtility.FromJson<BlendShapeList>(faceJson.ToString());
        _eyebrow = JsonUtility.FromJson<BlendShapeList>(eyebrowJson.ToString());
        _eyelash = JsonUtility.FromJson<BlendShapeList>(eyelashJson.ToString());
        _lowerTeeth = JsonUtility.FromJson<BlendShapeList>(lowerTeethJson.ToString());
        _upperTeeth = JsonUtility.FromJson<BlendShapeList>(upperTeethJson.ToString());

        foreach (string bsName in bSCategorynames.categoryNames)
        {
            string currentBSName = bsName.ToLower();
            foreach (BlendShapeInfo bsi in _face.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    faceDictionary.Add(currentBSName, bsi.Index);
                }
            }
            foreach (BlendShapeInfo bsi in _eyebrow.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    eyebrowDictionary.Add(currentBSName, bsi.Index);
                }
            }
            foreach (BlendShapeInfo bsi in _eyelash.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    eyelashDictionary.Add(currentBSName, bsi.Index);
                }
            }
            foreach (BlendShapeInfo bsi in _upperTeeth.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    UpperTeethDictionary.Add(currentBSName, bsi.Index);
                }
            }
            foreach (BlendShapeInfo bsi in _lowerTeeth.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    lowerTeethDictionary.Add(currentBSName, bsi.Index);
                }
            }
        }
    }

    public void ApplyDataOnFace(Classifications data)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (Category cat in data.categories)
            {
                //Debug.Log("BS " + cat.categoryName + " Score = " + (cat.score * 100));
                if (faceDictionary.TryGetValue(cat.categoryName.ToLower(), out int faceIndex))
                {
                    face.SetBlendShapeWeight(faceIndex, cat.score * 100);
                }
                if (eyebrowDictionary.TryGetValue(cat.categoryName.ToLower(), out int eyebrowIndex))
                {
                    eyebrow.SetBlendShapeWeight(eyebrowIndex, cat.score * 100);
                }
                if (eyelashDictionary.TryGetValue(cat.categoryName.ToLower(), out int eyelashIndex))
                {
                    eyelash.SetBlendShapeWeight(eyelashIndex, cat.score * 100);
                }
                if (UpperTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out int upperTeethIndex))
                {
                    upperTeeth.SetBlendShapeWeight(upperTeethIndex, cat.score * 100);
                }
                if (lowerTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out int lowerTeethIndex))
                {
                    lowerteeth.SetBlendShapeWeight(lowerTeethIndex, cat.score * 100);
                }
            }
        });
    }

    public void SetFaceRotation(List<Matrix4x4> facialTransformationMatrices)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Log the number of matrices
            //Debug.Log("Number of Matrices: " + facialTransformationMatrices.Count);

            // Get the list of rotations
            List<Quaternion> rotations = FacialRotationUtil.GetRotationsFromMatrices(facialTransformationMatrices);

            // Log the number of rotations
            //Debug.Log("Number of Rotations: " + rotations.Count);

            // Apply the first rotation to the GameObject (for example purposes)
            if (rotations.Count > 0)
            {
                targetRotation = rotations[0];
                //targetRotation = Quaternion.EulerAngles(rotations[0].x, rotations[0].y + 180, rotations[0].z);
                //Debug.Log("Target Rotation: " + targetRotation.eulerAngles.ToString());
            }

            // Smoothly interpolate to the target rotation
            //faceObject.transform.rotation = Quaternion.Slerp(faceObject.transform.rotation, targetRotation, rotationSmoothing);
            faceObject.transform.rotation = targetRotation;
            //animator.SetBoneLocalRotation(HumanBodyBones.Neck, targetRotation);
            //Debug.Log("Current Rotation: " + faceObject.transform.rotation.eulerAngles.ToString());
        });
    }

    void OnAnimatorIK(int layerIndex)
    {
        animator.SetBoneLocalRotation(HumanBodyBones.Neck, faceObject.transform.localRotation);
    }
}

[System.Serializable]
public class BlendShapeInfo
{
    public int Index;
    public string Name;
}

[System.Serializable]
public class BlendShapeList
{
    public List<BlendShapeInfo> BlendShapes;
}

[System.Serializable]
public class BSCategorynames
{
    public List<string> categoryNames;
}