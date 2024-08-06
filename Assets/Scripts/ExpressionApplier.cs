using System.Collections.Generic;
using System.Linq;
using Arnab.Scripts;
using Mediapipe.Tasks.Components.Containers;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;

/// <summary>
/// This class is responsible for applying expressions to a character from blendshape scores.
/// </summary>
public class ExpressionApplier : MonoBehaviour
{
    public static ExpressionApplier Instance;

    [SerializeField] private SkinnedMeshRenderer face, eyebrow, eyelash;
    [SerializeField] private SkinnedMeshRenderer lowerTeeth;
    [SerializeField] private SkinnedMeshRenderer upperTeeth;
    [SerializeField] private TextAsset faceJson, eyebrowJson, eyelashJson, lowerTeethJson, upperTeethJson, bsKeyJson;
    [SerializeField] private GameObject faceObject;

    private readonly Dictionary<string, int> _faceDictionary = new();
    private readonly Dictionary<string, int> _eyelashDictionary = new();
    private readonly Dictionary<string, int> _eyebrowDictionary = new();
    private readonly Dictionary<string, int> _lowerTeethDictionary = new();
    private readonly Dictionary<string, int> _upperTeethDictionary = new();

    private DataStructures.BsCategoryNames _bSCategoryNames = new();
    private DataStructures.BlendShapeList _face = new();
    private DataStructures.BlendShapeList _eyebrow = new();
    private DataStructures.BlendShapeList _eyelash = new();
    private DataStructures.BlendShapeList _lowerTeeth = new();
    private DataStructures.BlendShapeList _upperTeeth = new();

    // public float rotationSmoothing = 1f; // Adjust this for smoothing

    private Quaternion _targetRotation;

    public Animator animator;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _bSCategoryNames = JsonUtility.FromJson<DataStructures.BsCategoryNames>(bsKeyJson.ToString());
        _face = JsonUtility.FromJson<DataStructures.BlendShapeList>(faceJson.ToString());
        _eyebrow = JsonUtility.FromJson<DataStructures.BlendShapeList>(eyebrowJson.ToString());
        _eyelash = JsonUtility.FromJson<DataStructures.BlendShapeList>(eyelashJson.ToString());
        _lowerTeeth = JsonUtility.FromJson<DataStructures.BlendShapeList>(lowerTeethJson.ToString());
        _upperTeeth = JsonUtility.FromJson<DataStructures.BlendShapeList>(upperTeethJson.ToString());

        foreach (var bsName in _bSCategoryNames.categoryNames)
        {
            var currentBsName = bsName.ToLower();
            
            foreach (var bsi in _face.BlendShapes.Where(bsi => bsi.Name.ToLower().Contains(currentBsName)))
                _faceDictionary.Add(currentBsName, bsi.Index);

            foreach (var bsi in _eyebrow.BlendShapes.Where(bsi => bsi.Name.ToLower().Contains(currentBsName)))
                _eyebrowDictionary.Add(currentBsName, bsi.Index);
            
            foreach (var bsi in _eyelash.BlendShapes.Where(bsi => bsi.Name.ToLower().Contains(currentBsName)))
                _eyelashDictionary.Add(currentBsName, bsi.Index);
            
            foreach (var bsi in _upperTeeth.BlendShapes.Where(bsi => bsi.Name.ToLower().Contains(currentBsName)))
                _upperTeethDictionary.Add(currentBsName, bsi.Index);

            foreach (var bsi in _lowerTeeth.BlendShapes.Where(bsi => bsi.Name.ToLower().Contains(currentBsName)))
                _lowerTeethDictionary.Add(currentBsName, bsi.Index);
        }
    }

    public void ApplyDataOnFace(Classifications data)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (var cat in data.categories)
            {
                //Debug.Log("BS " + cat.categoryName + " Score = " + (cat.score * 100));
                if (_faceDictionary.TryGetValue(cat.categoryName.ToLower(), out var faceIndex))
                    face.SetBlendShapeWeight(faceIndex, cat.score * 100);
                
                if (_eyebrowDictionary.TryGetValue(cat.categoryName.ToLower(), out var eyebrowIndex))
                    eyebrow.SetBlendShapeWeight(eyebrowIndex, cat.score * 100);
                
                if (_eyelashDictionary.TryGetValue(cat.categoryName.ToLower(), out var eyelashIndex))
                    eyelash.SetBlendShapeWeight(eyelashIndex, cat.score * 100);
                
                if (_upperTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out var upperTeethIndex))
                    upperTeeth.SetBlendShapeWeight(upperTeethIndex, cat.score * 100);
                
                if (_lowerTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out var lowerTeethIndex))
                    lowerTeeth.SetBlendShapeWeight(lowerTeethIndex, cat.score * 100);
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
            var rotations = FacialRotationUtil.GetRotationsFromMatrices(facialTransformationMatrices);

            // Log the number of rotations
            //Debug.Log("Number of Rotations: " + rotations.Count);

            // Apply the first rotation to the GameObject (for example purposes)
            if (rotations.Count > 0)
            {
                _targetRotation = rotations[0];
                //targetRotation = Quaternion.EulerAngles(rotations[0].x, rotations[0].y + 180, rotations[0].z);
                //Debug.Log("Target Rotation: " + targetRotation.eulerAngles.ToString());
            }

            // Smoothly interpolate to the target rotation
            //faceObject.transform.rotation = Quaternion.Slerp(faceObject.transform.rotation, targetRotation, rotationSmoothing);
            faceObject.transform.rotation = _targetRotation;
            //animator.SetBoneLocalRotation(HumanBodyBones.Neck, targetRotation);
            //Debug.Log("Current Rotation: " + faceObject.transform.rotation.eulerAngles.ToString());
        });
    }

    private void OnAnimatorIK(int layerIndex) 
        => animator.SetBoneLocalRotation(HumanBodyBones.Neck, faceObject.transform.localRotation);
}