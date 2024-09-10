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

    public Dictionary<string, int> faceDictionaryAll = new Dictionary<string, int>();
    public Dictionary<string, int> eyelashDictionaryAll = new Dictionary<string, int>();

    BSCategorynames bSCategorynames = new BSCategorynames();
    BlendShapeList _eyebrow = new BlendShapeList();
    BlendShapeList _face = new BlendShapeList();
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
        _eyelash = JsonUtility.FromJson<BlendShapeList>(eyelashJson.ToString());
        _eyebrow = JsonUtility.FromJson<BlendShapeList>(eyebrowJson.ToString());
        _lowerTeeth = JsonUtility.FromJson<BlendShapeList>(lowerTeethJson.ToString());
        _upperTeeth = JsonUtility.FromJson<BlendShapeList>(upperTeethJson.ToString());

        foreach (BlendShapeInfo bsi in _face.BlendShapes)
        {
            try { faceDictionaryAll.Add(bsi.Name.ToLower(), bsi.Index); } catch { Debug.Log("face " + bsi.Name); }
        }

        foreach (BlendShapeInfo bsi in _eyelash.BlendShapes)
        {
            try { eyelashDictionaryAll.Add(bsi.Name.ToLower(), bsi.Index); } catch { Debug.Log("eyelash " + bsi.Name); }
        }

        foreach (string bsName in bSCategorynames.categoryNames)
        {
            string currentBSName = bsName.ToLower();
            foreach (BlendShapeInfo bsi in _face.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    try { faceDictionary.Add(currentBSName, bsi.Index); } catch { }
                }

            }
            foreach (BlendShapeInfo bsi in _eyebrow.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    try { eyebrowDictionary.Add(currentBSName, bsi.Index); } catch { }
                }
            }
            foreach (BlendShapeInfo bsi in _eyelash.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    try { eyelashDictionary.Add(currentBSName, bsi.Index); } catch { }
                }
            }
            foreach (BlendShapeInfo bsi in _upperTeeth.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    try { UpperTeethDictionary.Add(currentBSName, bsi.Index); } catch { }
                }
            }
            foreach (BlendShapeInfo bsi in _lowerTeeth.BlendShapes)
            {
                if (bsi.Name.ToLower().Contains(currentBSName))
                {
                    try { lowerTeethDictionary.Add(currentBSName, bsi.Index); } catch { }
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
                if (eyelashDictionary.TryGetValue(cat.categoryName.ToLower(), out int _eyelashIndex))
                {
                    eyelash.SetBlendShapeWeight(_eyelashIndex, cat.score * 100);
                }
                if (UpperTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out int upperTeethIndex))
                {
                    upperTeeth.SetBlendShapeWeight(upperTeethIndex, cat.score * 100);
                }
                if (lowerTeethDictionary.TryGetValue(cat.categoryName.ToLower(), out int lowerTeethIndex))
                {
                    lowerteeth.SetBlendShapeWeight(lowerTeethIndex, cat.score * 100);
                }
                if (cat.categoryName.ToLower().Contains("eyeblinkleft"))
                {
                    string currentActiveEye = EyeShapeManager.Instance.activeEyeShape;
                    int eyelashIndex = -1;
                    float val = cat.score * 100;
                    switch (currentActiveEye)
                    {
                        case "Tsundere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Tsundere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Tsundere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Tsundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Kudere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Kundere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Kudere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Kundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Deredere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Deredere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Deredere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Deredere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Dandere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Dandere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Dandere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Dandere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeTriangle":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Triangle".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeTriangle_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Triangle".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_01":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Round_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_01_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Round_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_02":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Round_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_02_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Round_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeUpturned":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Upturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeUpturned_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Upturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeDownturned":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Downturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeDownturned_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Downturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Almond_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Almond_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_02":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Almond_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_02_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Almond_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeHonkai":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Honkai".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeHonkai_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Honkai".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyePokemon":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Pokemon".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyePokemon_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Pokemon".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeSakura":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_Kundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeSakura_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkLeft_CC_Sakura".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        default:
                            //face.SetBlendShapeWeight(mapping.index, val);
                            break;
                    }
                }
                else if (cat.categoryName.ToLower().Contains("eyeblinkright"))
                {
                    string currentActiveEye = EyeShapeManager.Instance.activeEyeShape;
                    int eyelashIndex = -1;
                    float val = cat.score * 100;
                    switch (currentActiveEye)
                    {
                        case "Tsundere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Tsundere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Tsundere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Tsundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Kudere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Kundere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Kudere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Kundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Deredere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Deredere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Deredere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Deredere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Dandere_Female":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Dandere_Female".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "Dandere_Male":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Dandere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeTriangle":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Triangle".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeTriangle_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Triangle".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_01":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Round_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_01_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Round_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_02":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Round_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeRound_02_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Round_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeUpturned":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Upturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeUpturned_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Upturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeDownturned":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Downturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeDownturned_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Downturned".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Almond_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Almond_01".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_02":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Almond_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeAlmond_02_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Almond_02".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeHonkai":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Honkai".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeHonkai_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Honkai".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyePokemon":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Pokemon".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyePokemon_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Pokemon".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeSakura":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_Kundere_Male".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        case "EyeSakura_2":
                            if (faceDictionaryAll.TryGetValue("ArKit_Face.eyeBlinkRight_CC_Sakura".ToLower(), out faceIndex))
                            {
                                face.SetBlendShapeWeight(faceIndex, val);
                            }
                            break;
                        default:
                            // You can handle a default case here if needed
                            // For example, if you want to handle an unknown blendshape name
                            break;
                    }
                }
                if (cat.categoryName.ToLower().Contains("eyeblinkright"))
                {
                    string currentActiveEye = EyeShapeManager.Instance.activeEyeShape;
                    int eyelashIndex = -1;
                    float val = cat.score * 100;
                    switch (currentActiveEye)
                    {
                        case "Tsundere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Tsundere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Tsundere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Tsundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Kudere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Kundere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Kudere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Kundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Deredere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Deredere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Deredere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Deredere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Dandere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Dandere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Dandere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Dandere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeTriangle":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Triangle".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeTriangle_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Triangle".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_01":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Round_01".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_01_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Round_01".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_02":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Round_02".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_02_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Round_02".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeUpturned":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Upturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeUpturned_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Upturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeDownturned":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Downturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeDownturned_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Downturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Almond".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Almond".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_02":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Almond_2".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_02_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Almond_2".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeHonkai":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Honkai".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeHonkai_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Honkai".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyePokemon":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Pokemon".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyePokemon_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Pokemon".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeSakura":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Kundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeSakura_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkRight_Sakura".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        default:
                            // You can handle a default case here if needed
                            // For example, if you want to handle an unknown blendshape name
                            break;
                    }

                }
                else if (cat.categoryName.ToLower().Contains("eyeblinkleft"))
                {
                    string currentActiveEye = EyeShapeManager.Instance.activeEyeShape;
                    int eyelashIndex = -1;
                    float val = cat.score * 100;
                    switch (currentActiveEye)
                    {
                        case "Tsundere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Tsundere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Tsundere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Tsundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Kudere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Kundere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Kudere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Kundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Deredere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Deredere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Deredere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Deredere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Dandere_Female":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Dandere_Female".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "Dandere_Male":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Dandere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeTriangle":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Triangle".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeTriangle_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Triangle".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_01":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Round_01".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_01_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Round_01".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_02":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Round_02".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeRound_02_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Round_02".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeUpturned":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Upturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeUpturned_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Upturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeDownturned":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Downturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeDownturned_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Downturned".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Almond".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Almond".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_02":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Almond_2".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeAlmond_02_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Almond_2".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeHonkai":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Honkai".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeHonkai_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Honkai".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyePokemon":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeftt_Pokemon".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyePokemon_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeftt_Pokemon".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeSakura":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Kundere_Male".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        case "EyeSakura_2":
                            if (eyelashDictionaryAll.TryGetValue("ArKit_Eyelash.eyeBlinkLeft_Sakura".ToLower(), out eyelashIndex))
                            {
                                eyelash.SetBlendShapeWeight(eyelashIndex, val);
                            }
                            break;
                        default:
                            // You can handle a default case here if needed
                            // For example, if you want to handle an unknown blendshape name
                            break;
                    }
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