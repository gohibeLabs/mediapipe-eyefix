using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionBlendShapesController : MonoBehaviour
{
    public List<string> bsNames = new List<string>{
    "BrowDownLeft",
    "BrowDownRight",
    "BrowInnerUp",
    "BrowOuterUpLeft",
    "BrowOuterUpRight",
    "CheekPuff",
    "CheekSquintLeft",
    "CheekSquintRight",
    "EyeBlinkLeft",
    "EyeBlinkRight",
    "EyeLookDownLeft",
    "EyeLookDownRight",
    "EyeLookInLeft",
    "EyeLookInRight",
    "EyeLookOutLeft",
    "EyeLookOutRight",
    "EyeLookUpLeft",
    "EyeLookUpRight",
    "EyeSquintLeft",
    "EyeSquintRight",
    "EyeWideLeft",
    "EyeWideRight",
    "JawForward",
    "JawLeft",
    "JawOpen",
    "JawRight",
    "MouthClose",
    "MouthDimpleLeft",
    "MouthDimpleRight",
    "MouthFrownLeft",
    "MouthFrownRight",
    "MouthFunnel",
    "MouthLeft",
    "MouthLowerDownLeft",
    "MouthLowerDownRight",
    "MouthPressLeft",
    "MouthPressRight",
    "MouthPucker",
    "MouthRight",
    "MouthRollLower",
    "MouthRollUpper",
    "MouthShrugLower",
    "MouthShrugUpper",
    "MouthSmileLeft",
    "MouthSmileRight",
    "MouthStretchLeft",
    "MouthStretchRight",
    "MouthUpperUpLeft",
    "MouthUpperUpRight",
    "NoseSneerLeft",
    "NoseSneerRight",
    "TongueOut"
    };

    public Dictionary<string, OneEuroFilter> bsFilters = new Dictionary<string, OneEuroFilter>();

    private static ExpressionBlendShapesController Instance;

    [SerializeField] private double minCutoff = 1.0;
    [SerializeField] private double beta = 0.007;
    [SerializeField] private double dCutoff = 1.0;
    [SerializeField] private double minInput = 0;
    [SerializeField] private double maxInput = 1;
    [SerializeField] private double minOutput = 0;
    [SerializeField] private double maxOutput = 1;
    [SerializeField] private bool isOneEuro = true;
    [SerializeField] private double multiplier = 1;

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

    public static ExpressionBlendShapesController GetInstance()
    {
        return Instance;
    }

    private void Start()
    {
        InitiateFilters();
    }

    public float GetBlendsapeValueFromFilter(string bsKey, float value)
    {
        OneEuroFilter temp;
        if (bsFilters.TryGetValue(bsKey, out temp))
        {
            double returnedvalue = temp.CalculateValue((double)value);
            if (!double.IsNaN(returnedvalue))
            {
                Debug.LogError(bsKey);
                return (float)returnedvalue;
            }
        }
        return value;

    }
    [ContextMenu("Change Values")]
    public void InitiateFilters()
    {
        bsFilters.Clear();
        foreach (string bsName in bsNames)
        {
            OneEuroFilter temp = new OneEuroFilter(minCutoff, beta, minInput, maxInput, minOutput, maxOutput, isOneEuro, multiplier, dCutoff);
            bsFilters.Add(bsName.ToLower(), temp);
        }
    }

    public void SetValues(double minCutoff, double beta, double minInput, double maxInput, double minOutput, double maxOutput, bool isOneEuro, double multiplier, double dCutoff = 1.0, double freq = 120.0)
    {
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.minInput = minInput;
        this.maxInput = maxInput;
        this.minOutput = minOutput;
        this.maxOutput = maxOutput;
        this.isOneEuro = isOneEuro;
        this.multiplier = multiplier;
        this.dCutoff = dCutoff;
        InitiateFilters();
        //gameObject.SetActive(false);
    }
}
