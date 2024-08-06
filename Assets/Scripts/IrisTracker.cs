using System.Collections.Generic;
using Arnab.Scripts;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;
using PimDeWitte.UnityMainThreadDispatcher;

/// <summary>
/// This class tracks iris movement and applies rotations to eyeballs accordingly.
/// </summary>
public class IrisTracker : MonoBehaviour
{
    #region Fields & References
    
    /// <summary>
    /// Static instance of class.
    /// </summary>
    public static IrisTracker Instance;
    
    /// <summary>
    /// Left eye bone transform reference.
    /// </summary>
    [SerializeField] private Transform leftEyeBone;
    
    /// <summary>
    /// Right eye bone transform reference.
    /// </summary>
    [SerializeField] private Transform rightEyeBone;

    /// <summary>
    /// Left eye min-max range values for clamping Y-axis rotation angles.
    /// </summary>
    [SerializeField] [Range(-70, 70)] private int[] leftEyeAngleRangeInOut = {-10, 10};
    
    /// <summary>
    /// Left eye Y-axis rotation angle offset.
    /// </summary>
    [SerializeField] [Range(-5, 5)] private float leftEyeYAngleOffset = 3.2857f;
    
    /// <summary>
    /// Right eye min-max range values for clamping Y-axis rotation angles.
    /// </summary>
    [SerializeField] [Range(-70, 70)] private int[] rightEyeAngleRangeInOut = {-10, 10};
    
    /// <summary>
    /// Right eye Y-axis rotation angle offset.
    /// </summary>
    [SerializeField] [Range(-5, 5)] private float rightEyeYAngleOffset = -3.2857f;
    
    /// <summary>
    /// Left & Right (Both) eye min-max range values for clamping X-axis rotation angles.
    /// </summary>
    [SerializeField] [Range(-30, 30)] private int[] bothEyeAngleRangeUpDown = {-15, 7};
    
    /// <summary>
    /// Angle multiplication factor for normalized iris positions for up-down movement.
    /// </summary>
    [SerializeField] private float angleMultiplierUpDown = 60f;
    
    /// <summary>
    /// Angle multiplication factor for normalized iris positions for in-out movement.
    /// </summary>
    [SerializeField] private float angleMultiplierInOut = 80f;

    /// <summary>
    /// Nose bridge top point position, in world space.
    /// Used for cross-eyed angle multiplier correction.
    /// </summary>
    private Vector3 _noseTopPos;
    
    /// <summary>
    /// Nose bridge bottom point position, in world space.
    /// /// Used for cross-eyed angle multiplier correction.
    /// </summary>
    private Vector3 _noseBottomPos;

    // Indices for landmarks.
    private const int NoseTopIndex = 6; // Nose bridge top point.
    private const int NoseBottomIndex = 2; // Nose bridge bottom point.
    
    private const int LeftIrisIndex = 473; // Center of left iris.
    private const int LeftEyeInnerMostIndex = 362; // 463; // Inner corner of left eye.
    private const int LeftEyeOuterMostIndex = 263; // Outer corner of left eye.
    private const int LeftEyeTopMostIndex = 386; // Top most point of left eye.
    private const int LeftEyeBottomMostIndex = 374; // Bottom most point of left eye.

    private const int RightIrisIndex = 468; // Center of right iris.
    private const int RightEyeInnerMostIndex = 133; // Inner corner of right eye.
    private const int RightEyeOuterMostIndex = 33; // Outer corner of right eye.
    private const int RightEyeTopMostIndex = 159; // Top most point of right eye.
    private const int RightEyeBottomMostIndex = 145; // Bottom most point of right eye.
    
    #endregion Fields & References

    #region Methods
    
    /// <summary>
    /// Ensure single instance.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Tracks iris movement relative to facial rotation.
    /// Sets eyeball angles relative to iris movement, within limits. 
    /// </summary>
    /// <param name="faceLandmarks">Input facial landmarks, including iris points.</param>
    public void TrackIrisMovement(List<NormalizedLandmarks> faceLandmarks)
    {
        if (faceLandmarks == null || faceLandmarks.Count == 0)
        {
            Debug.Log("No face landmarks detected.");
            return;
        }

        // Cache indexed landmark element for efficiency and ease of change.
        var landmarks = faceLandmarks[0].landmarks.ToArray();

        // Track the nose bridge top and bottom points for cross-eye angle correction.
        _noseTopPos = Utilities.ConvertToVector3(landmarks[NoseTopIndex]);
        _noseBottomPos = Utilities.ConvertToVector3(landmarks[NoseBottomIndex]);
        
        // Get an eye data struct for both eyes, populated by landmarks and their indices.
        var leftEyeData = GetEyeData(landmarks, LeftIrisIndex, LeftEyeInnerMostIndex, LeftEyeOuterMostIndex,
            LeftEyeTopMostIndex, LeftEyeBottomMostIndex);
        
        var rightEyeData = GetEyeData(landmarks, RightIrisIndex, RightEyeInnerMostIndex, RightEyeOuterMostIndex,
            RightEyeTopMostIndex, RightEyeBottomMostIndex);

        // After all calculations, apply all eye rotations in main thread.
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ApplyEyeRotations(leftEyeData, rightEyeData, _noseTopPos, _noseBottomPos);
        });
    }

    /// <summary>
    /// Applies in-out and up-down rotations to both eyes.
    /// </summary>
    /// <param name="leftEyeData">Input left eye dataset.</param>
    /// <param name="rightEyeData">Input right eye dataset.</param>
    /// <param name="noseTopPos">Nose bridge top point world position.</param>
    /// <param name="noseBottomPos">Nose bridge bottom point world position.</param>
    private void ApplyEyeRotations(DataStructures.EyeData leftEyeData, DataStructures.EyeData rightEyeData,
        Vector3 noseTopPos, Vector3 noseBottomPos)
    {
        var (verticalTiltFactor, horizontalTiltFactor) = CalculateTiltFactors(noseTopPos, noseBottomPos,
            leftEyeData.OuterMost, rightEyeData.OuterMost);
        
        // First we calculate in-out (Y-axis) rotation euler angles for both eyes.
        var leftRotation = CalculateInOutEyeRotation(leftEyeData, leftEyeYAngleOffset, -1, horizontalTiltFactor);
        var rightRotation = CalculateInOutEyeRotation(rightEyeData, rightEyeYAngleOffset, 1, horizontalTiltFactor);

        // Then we calculate up-down (X-axis) rotation euler angles for both eyes.
        var upDownAngle = GetIrisBasedAngle(rightEyeData.IrisCenter, 
            rightEyeData.TopMost, rightEyeData.BottomMost, 1, angleMultiplierUpDown,
            verticalTiltFactor);

        // Finally we combine calculated local euler angles for both X and Y axes and smoothly apply rotations to both eyes.
        Utilities.ApplyClampedSmoothRotation(leftEyeBone, new Vector3(upDownAngle, leftRotation.y, 0),
            bothEyeAngleRangeUpDown, leftEyeAngleRangeInOut);

        // To prevent differences, apply left eye rotation to right eye rotation.
        rightEyeBone.localRotation = leftEyeBone.localRotation;

        // Utilities.ApplyClampedSmoothRotation(rightEyeBone, new Vector3(upDownAngle, rightRotation.y, 0),
        //     bothEyeAngleRangeUpDown, rightEyeAngleRangeInOut);
    }

    /// <summary>
    /// Calculate eyeball rotation from eye data.
    /// </summary>
    /// <param name="eyeData">Input eye data struct.</param>
    /// <param name="yAngleOffset">Input Y-axis rotation angle offset.</param>
    /// <param name="dirSign">Input direction sign for angle.</param>
    /// <param name="horizontalTiltFactor">Horizontal head tile factor.</param>
    /// <returns>Returns in-out eyeball rotation angle (Y-axis rotation) for input eye.</returns>
    private Vector3 CalculateInOutEyeRotation(DataStructures.EyeData eyeData, float yAngleOffset, float dirSign,
        float horizontalTiltFactor)
    {
        var inOutAngle = GetIrisBasedAngle(eyeData.IrisCenter, eyeData.InnerMost,
            eyeData.OuterMost, dirSign, angleMultiplierInOut, horizontalTiltFactor);
        
        return new Vector3(0, inOutAngle + yAngleOffset, 0);
    }

    /// <summary>
    /// Get the eyeball angle based on current normalized position of iris relative to extreme eye points.
    /// </summary>
    /// <param name="irisCenter">World position of iris.</param>
    /// <param name="minPoint">Minimum world position for normalized position calculation.</param>
    /// <param name="maxPoint">Maximum world position for normalized position calculation.</param>
    /// <param name="dirSign">The Sign (1 or -1) for inverting direction during calculation.</param>
    /// <param name="angleMultiplier">Multiplication factor for resultant normalized iris position.</param>
    /// <param name="tiltFactor">Head tile factor.</param>
    /// <returns>The local euler angle of eyeball around min-max points direction axis.</returns>
    private float GetIrisBasedAngle(Vector3 irisCenter, Vector3 minPoint, Vector3 maxPoint, float dirSign, float angleMultiplier,
        float tiltFactor)
    {
        // First we find the half of distance between min and max point in world space.
        var distHalf = Vector3.Distance(minPoint, maxPoint) / 2f;
        
        // Get direction vector formed by min and max point.
        var dir = (maxPoint - minPoint) * dirSign;
        
        // Calculate the central point between min and max, in world space.
        var center = Vector3.Lerp(minPoint, maxPoint, 0.5f);
        
        // The min, max and iris positions form an imaginary triangle in world space.
        // Find the projected iris point (As if iris point is made to perpendicularly intersect with min-max direction vector line).
        var projectedIrisPoint = center + Vector3.Project(irisCenter - center, dir);
        
        // Then calculate iris's direction sign, to know if iris moves leftward or rightward when away from center point.
        var irisDirSign = Mathf.Sign(Vector3.Dot(projectedIrisPoint - center, dir));
        
        // Normalized position of iris relative to min/max point.
        // A value of 0 means iris is at center. A value of 0.5f means iris is at one extreme end.
        var normalizedPos = Vector3.Distance(projectedIrisPoint, center) / distHalf;

        // Adjust angle multiplier based on both vertical and horizontal tilt.
        // This is important to resolve the cross-eyed look when tilting head on any axis.
        var adjustedMultiplier = angleMultiplier * tiltFactor;

        // Return final angle for eyeball.
        // Final angle is calculated by multiplying normalized iris position (0 to 0.5f),
        // with the direction sign calculated above, and finally with angle multiplier for this eyeball.
        // The direction sign lets us know if final angle should be positive or negative.
        return irisDirSign * adjustedMultiplier * normalizedPos;
    }
    
    /// <summary>
    /// Calculates tilt factors for head movement.
    /// This method helps fix cross-eyed look on head movement.
    /// </summary>
    /// <param name="noseTop">Nose bridge top point world position.</param>
    /// <param name="noseBottom">Nose bridge bottom point world position.</param>
    /// <param name="leftEye">Left eye outer most point world position.</param>
    /// <param name="rightEye">Right eye outer most point world position.</param>
    /// <returns>A horizontal and vertical tilt factor tuple.</returns>
    private (float verticalTiltFactor, float horizontalTiltFactor) 
        CalculateTiltFactors(Vector3 noseTop, Vector3 noseBottom, Vector3 leftEye, Vector3 rightEye)
    {
        // Calculate vertical tilt using nose bridge top and bottom points.
        var noseDir = (noseTop - noseBottom).normalized;
        var upVector = Vector3.up;
        var verticalTiltAngle = Vector3.Angle(noseDir, upVector);
        var verticalTiltFactor = verticalTiltAngle / 90f - 1f; // 1 when upright, 0 when tilted 90 degrees.

        // Calculate horizontal tilt using left and right eye outer points.
        var eyeDir = (rightEye - leftEye).normalized;
        var rightVector = Vector3.right;
        var horizontalTiltAngle = Vector3.Angle(eyeDir, rightVector);
        var horizontalTiltFactor = horizontalTiltAngle / 90f - 1f; // 1 when level, 0 when tilted 90 degrees.

        return (verticalTiltFactor, horizontalTiltFactor);
    }
    
    /// <summary>
    /// Gets a Vector3 dataset representing world space tracking positions from landmark points, for an eye.
    /// </summary>
    /// <param name="landmarks">Input landmarks to pass in.</param>
    /// <param name="indices">Input indices of landmarks.</param>
    /// <returns>A Vector3 dataset struct  representing world space eye tracking positions.</returns>
    private DataStructures.EyeData GetEyeData(NormalizedLandmark[] landmarks, params int[] indices)
    {
        return new DataStructures.EyeData
        {
            IrisCenter = Utilities.ConvertToVector3(landmarks[indices[0]]),
            InnerMost = Utilities.ConvertToVector3(landmarks[indices[1]]),
            OuterMost = Utilities.ConvertToVector3(landmarks[indices[2]]),
            TopMost = Utilities.ConvertToVector3(landmarks[indices[3]]),
            BottomMost = Utilities.ConvertToVector3(landmarks[indices[4]])
        };
    }
    
    #endregion Methods
}