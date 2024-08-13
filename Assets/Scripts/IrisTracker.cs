// using System.Collections.Generic;
// using Arnab.Scripts;
// using UnityEngine;
// using Mediapipe.Tasks.Components.Containers;
// using PimDeWitte.UnityMainThreadDispatcher;
//
// /// <summary>
// /// This class tracks iris movement and applies rotations to eyeballs accordingly.
// /// </summary>
// public class IrisTracker : MonoBehaviour
// {
//     #region Fields & References
//
//     /// <summary>
//     /// Static instance of class.
//     /// </summary>
//     public static IrisTracker Instance;
//
//     /// <summary>
//     /// Left eye bone transform reference.
//     /// </summary>
//     [SerializeField] private Transform leftEyeBone;
//
//     /// <summary>
//     /// Right eye bone transform reference.
//     /// </summary>
//     [SerializeField] private Transform rightEyeBone;
//
//     /// <summary>
//     /// Left eye min-max range values for clamping Y-axis rotation angles.
//     /// </summary>
//     [SerializeField] [Range(-70, 70)] private int[] leftEyeAngleRangeInOut = {-38, 39};
//
//     /// <summary>
//     /// Left eye Y-axis rotation angle offset.
//     /// </summary>
//     [SerializeField] [Range(-5, 5)] private float leftEyeYAngleOffset;// = 3.2857f;
//
//     /// <summary>
//     /// Right eye min-max range values for clamping Y-axis rotation angles.
//     /// </summary>
//     [SerializeField] [Range(-70, 70)] private int[] rightEyeAngleRangeInOut = {-39, 38};
//
//     /// <summary>
//     /// Right eye Y-axis rotation angle offset.
//     /// </summary>
//     [SerializeField] [Range(-5, 5)] private float rightEyeYAngleOffset;// = -3.2857f;
//     
//     /// <summary>
//     /// Left eye Y-axis rotation angle offset relative to Right eye.
//     /// This makes the left and right eyeballs point slightly inward towards the nose.
//     /// This is called adjustment for eye angle kappa, just like real life.
//     /// </summary>
//     [SerializeField] [Range(-20, 20)] private float leftEyeOffsetFromRightEye = -15f;
//
//     /// <summary>
//     /// Left & Right (Both) eye min-max range values for clamping X-axis rotation angles.
//     /// </summary>
//     [SerializeField] [Range(-30, 30)] private int[] bothEyeAngleRangeUpDown = {-25, 30};
//
//     /// <summary>
//     /// Angle multiplication factor for normalized iris positions for up-down movement.
//     /// </summary>
//     [SerializeField] private float angleMultiplierUpDown = 60f;
//
//     /// <summary>
//     /// Angle multiplication factor for normalized iris positions for in-out movement.
//     /// </summary>
//     [SerializeField] private float angleMultiplierInOut = 78f;
//
//     /// <summary>
//     /// Multiplier for horizontal Y-axis head rotation based angle offset.
//     /// </summary>
//     [SerializeField] private float headOffsetHorizontalMultiplier = 1f;
//
//     /// <summary>
//     /// Multiplier for vertical X-axis head rotation based angle offset.
//     /// </summary>
//     [SerializeField] private float headOffsetVerticalMultiplier = 2f;
//
//     /// <summary>
//     /// Nose bridge top point position, in world space.
//     /// Used for cross-eyed angle multiplier correction.
//     /// </summary>
//     private Vector3 _noseTopPos;
//
//     /// <summary>
//     /// Nose bridge bottom point position, in world space.
//     /// /// Used for cross-eyed angle multiplier correction.
//     /// </summary>
//     private Vector3 _noseBottomPos;
//
//     // Indices for landmarks.
//     private const int NoseTopIndex = 6; // Nose bridge top point.
//     private const int NoseBottomIndex = 2; // Nose bridge bottom point.
//
//     private const int LeftIrisIndex = 473; // Center of left iris.
//     private const int LeftEyeInnerMostIndex = 362; // 463; // Inner corner of left eye.
//     private const int LeftEyeOuterMostIndex = 263; // Outer corner of left eye.
//     private const int LeftEyeTopMostIndex = 386; // Top most point of left eye.
//     private const int LeftEyeBottomMostIndex = 374; // Bottom most point of left eye.
//
//     private const int RightIrisIndex = 468; // Center of right iris.
//     private const int RightEyeInnerMostIndex = 133; // Inner corner of right eye.
//     private const int RightEyeOuterMostIndex = 33; // Outer corner of right eye.
//     private const int RightEyeTopMostIndex = 159; // Top most point of right eye.
//     private const int RightEyeBottomMostIndex = 145; // Bottom most point of right eye.
//
//     #endregion Fields & References
//
//     #region Methods
//
//     /// <summary>
//     /// Ensure single instance.
//     /// </summary>
//     private void Awake()
//     {
//         if (Instance == null)
//             Instance = this;
//         else
//             Destroy(gameObject);
//     }
//
//     /// <summary>
//     /// Tracks iris movement relative to facial rotation.
//     /// Sets eyeball angles relative to iris movement, within limits. 
//     /// </summary>
//     /// <param name="faceLandmarks">Input facial landmarks, including iris points.</param>
//     public void TrackIrisMovement(List<NormalizedLandmarks> faceLandmarks)
//     {
//         if (faceLandmarks == null || faceLandmarks.Count == 0)
//         {
//             Debug.Log("No face landmarks detected.");
//             return;
//         }
//
//         // Cache indexed landmark element for efficiency and ease of change.
//         var landmarks = faceLandmarks[0].landmarks.ToArray();
//
//         // Track the nose bridge top and bottom points for cross-eye angle correction.
//         _noseTopPos = Utilities.ConvertToVector3(landmarks[NoseTopIndex]);
//         _noseBottomPos = Utilities.ConvertToVector3(landmarks[NoseBottomIndex]);
//
//         // Get an eye data struct for both eyes, populated by landmarks and their indices.
//         var leftEyeData = GetEyeData(landmarks, LeftIrisIndex, LeftEyeInnerMostIndex, LeftEyeOuterMostIndex,
//             LeftEyeTopMostIndex, LeftEyeBottomMostIndex);
//
//         var rightEyeData = GetEyeData(landmarks, RightIrisIndex, RightEyeInnerMostIndex, RightEyeOuterMostIndex,
//             RightEyeTopMostIndex, RightEyeBottomMostIndex);
//
//         // After all calculations, apply all eye rotations in main thread.
//         UnityMainThreadDispatcher.Instance().Enqueue(() =>
//         {
//             ApplyEyeRotations(leftEyeData, rightEyeData, _noseTopPos, _noseBottomPos);
//         });
//     }
//
//     /// <summary>
//     /// Applies in-out and up-down rotations to both eyes.
//     /// </summary>
//     /// <param name="leftEyeData">Input left eye dataset.</param>
//     /// <param name="rightEyeData">Input right eye dataset.</param>
//     /// <param name="noseTopPos">Nose bridge top point world position.</param>
//     /// <param name="noseBottomPos">Nose bridge bottom point world position.</param>
//     private void ApplyEyeRotations(DataStructures.EyeLandMarkData leftEyeData, DataStructures.EyeLandMarkData rightEyeData,
//         Vector3 noseTopPos, Vector3 noseBottomPos)
//     {
//         // First we see how far off face rotation is from center rotation on both axes.
//         var (verticalTiltFactor, horizontalTiltFactor, horizontalSign, verticalSign) = CalculateTiltFactors(noseTopPos, noseBottomPos,
//             leftEyeData.OuterMost, rightEyeData.OuterMost);
//
//         // print(horizontalSign); // (W.r.t. avatar head) 1 = Looking right, -1 = Looking left.
//
//         var lookingRightSide = horizontalSign > 0;      // W.r.t avatar head.
//         
//         // Track the eye which will be the most visible depending on how head is turned.
//         var trackedEye = lookingRightSide ? leftEyeBone : rightEyeBone;
//         var trackedEyeData = lookingRightSide ? leftEyeData : rightEyeData;
//         var trackedEyeYOffset = -horizontalSign * leftEyeYAngleOffset;
//         var trackedEyeAngleMaxRange = lookingRightSide ? leftEyeAngleRangeInOut : rightEyeAngleRangeInOut;
//         
//         // The eye which will follow actively tracked eye.
//         var followingEye = lookingRightSide ? rightEyeBone : leftEyeBone;
//         var followingEyeAngleMaxRange = lookingRightSide ? rightEyeAngleRangeInOut : leftEyeAngleRangeInOut;
//         
//         // First we calculate in-out (Y-axis) rotation euler angles for both eyes.
//         var horizontalEyeRotation = CalculateInOutEyeRotation(trackedEyeData, trackedEyeYOffset, -horizontalSign,
//             horizontalTiltFactor, trackedEyeAngleMaxRange);
//         
//         // var leftRotation = CalculateInOutEyeRotation(trackedEyeData, -horizontalSign * leftEyeYAngleOffset, -1,
//         //     horizontalTiltFactor);
//         // var rightRotation = CalculateInOutEyeRotation(rightEyeData, rightEyeYAngleOffset, 1, horizontalTiltFactor);
//
//         // Then we calculate up-down (X-axis) rotation euler angles for both eyes.
//         // var upDownAngle = GetIrisBasedAngle(rightEyeData.IrisCenter, 
//         //     rightEyeData.TopMost, rightEyeData.BottomMost, 1, verticalSign * angleMultiplierUpDown,
//         //     verticalTiltFactor);
//
//         const int upDownAngle = 0;
//
//         // Finally we combine calculated local euler angles for both X and Y axes and smoothly apply rotations to both eyes.
//         Utilities.ApplyClampedSmoothRotation(trackedEye, new Vector3(upDownAngle, horizontalEyeRotation.y, 0),
//             bothEyeAngleRangeUpDown, leftEyeAngleRangeInOut);
//
//         // To prevent big deviations (cross-eyed or wide-eyed look) between the two eyeballs,
//         // apply tracked eye rotation to follower eye, with centering offset.
//
//         // var followingEyeRotEuler = trackedEye.localRotation.eulerAngles + new Vector3(0, 
//         //     Mathf.Clamp(horizontalSign * leftEyeOffsetFromRightEye, followingEyeAngleMaxRange[0], followingEyeAngleMaxRange[1]));
//         //
//         // print(followingEyeRotEuler.y);
//         //
//         // followingEye.localRotation = Quaternion.Euler(followingEyeRotEuler);
//         
//         followingEye.localRotation = trackedEye.localRotation * Quaternion.AngleAxis(horizontalSign * leftEyeOffsetFromRightEye, followingEye.up);
//         
//         print($"followingEye.localRotation: {followingEye.localRotation}, ");
//
//         // Utilities.ApplyClampedSmoothRotation(rightEyeBone, new Vector3(upDownAngle, rightRotation.y, 0),
//         //     bothEyeAngleRangeUpDown, rightEyeAngleRangeInOut);
//     }
//
//     /// <summary>
//     /// Calculate eyeball rotation from eye data.
//     /// </summary>
//     /// <param name="eyeData">Input eye data struct.</param>
//     /// <param name="yAngleOffset">Input Y-axis rotation angle offset.</param>
//     /// <param name="dirSign">Input direction sign for angle.</param>
//     /// <param name="horizontalTiltFactor">Horizontal head tile factor.</param>
//     /// <param name="minMaxAngleRange">Min-max angle range.</param>
//     /// <returns>Returns in-out eyeball rotation angle (Y-axis rotation) for input eye.</returns>
//     private Vector3 CalculateInOutEyeRotation(DataStructures.EyeLandMarkData eyeData, float yAngleOffset, float dirSign,
//         float horizontalTiltFactor, int[] minMaxAngleRange)
//     {
//         var inOutAngle = Mathf.Clamp(GetIrisBasedAngle(eyeData.IrisCenter, eyeData.InnerMost,
//             eyeData.OuterMost, dirSign, angleMultiplierInOut, horizontalTiltFactor), minMaxAngleRange[0], minMaxAngleRange[1]);
//
//         return new Vector3(0, inOutAngle + yAngleOffset, 0);
//     }
//
//     /// <summary>
//     /// Get the eyeball angle based on current normalized position of iris relative to extreme eye points.
//     /// </summary>
//     /// <param name="irisCenter">World position of iris.</param>
//     /// <param name="minPoint">Minimum world position for normalized position calculation.</param>
//     /// <param name="maxPoint">Maximum world position for normalized position calculation.</param>
//     /// <param name="dirSign">The Sign (1 or -1) for inverting direction during calculation.</param>
//     /// <param name="angleMultiplier">Multiplication factor for resultant normalized iris position.</param>
//     /// <param name="tiltFactor">Head tile factor.</param>
//     /// <returns>The local euler angle of eyeball around min-max points direction axis.</returns>
//     private float GetIrisBasedAngle(Vector3 irisCenter, Vector3 minPoint, Vector3 maxPoint, float dirSign,
//         float angleMultiplier,
//         float tiltFactor)
//     {
//         // First we find the half of distance between min and max point in world space.
//         var distHalf = Vector3.Distance(minPoint, maxPoint) / 2f;
//
//         // Get direction vector formed by min and max point.
//         var dir = (maxPoint - minPoint) * dirSign;
//
//         // Calculate the central point between min and max, in world space.
//         var center = Vector3.Lerp(minPoint, maxPoint, 0.5f);
//
//         // The min, max and iris positions form an imaginary triangle in world space.
//         // Find the projected iris point (As if iris point is made to perpendicularly intersect with min-max direction vector line).
//         var projectedIrisPoint = center + Vector3.Project(irisCenter - center, dir);
//
//         // Then calculate iris's direction sign, to know if iris moves leftward or rightward when away from center point.
//         var irisDirSign = Mathf.Sign(Vector3.Dot(projectedIrisPoint - center, dir));
//
//         // Normalized position of iris relative to min/max point.
//         // A value of 0 means iris is at center. A value of 0.5f means iris is at one extreme end.
//         var normalizedPos = Vector3.Distance(projectedIrisPoint, center) / distHalf;
//         
//         // print(normalizedPos);
//
//         // Adjust angle multiplier based on both vertical and horizontal tilt.
//         // This is important to resolve the cross-eyed look when tilting head on any axis.
//         var adjustedMultiplier = angleMultiplier * tiltFactor;
//
//         // Return final angle for eyeball.
//         // Final angle is calculated by multiplying normalized iris position (0 to 0.5f),
//         // with the direction sign calculated above, and finally with angle multiplier for this eyeball.
//         // The direction sign lets us know if final angle should be positive or negative.
//         return irisDirSign * adjustedMultiplier * normalizedPos;
//     }
//
//     /// <summary>
//     /// Calculates tilt factors for head movement.
//     /// This method helps fix cross-eyed look on head movement.
//     /// </summary>
//     /// <param name="noseTop">Nose bridge top point world position.</param>
//     /// <param name="noseBottom">Nose bridge bottom point world position.</param>
//     /// <param name="leftEye">Left eye outermost point world position.</param>
//     /// <param name="rightEye">Right eye outermost point world position.</param>
//     /// <returns>A horizontal and vertical tilt factor tuple.</returns>
//     private (float verticalTiltFactor, float horizontalTiltFactor, float horizontalSign, float verticalSign)
//         CalculateTiltFactors(Vector3 noseTop, Vector3 noseBottom, Vector3 leftEye, Vector3 rightEye)
//     {
//         // Calculate vertical tilt using nose bridge top and bottom points.
//         var noseDir = (noseTop - noseBottom).normalized;
//         var upVector = Vector3.up;
//         var verticalTiltAngle = Vector3.Angle(noseDir, upVector);
//         var verticalTiltSignedAngle = Vector3.SignedAngle(noseDir, upVector, Vector3.right);
//         var verticalSign = Mathf.Sign(verticalTiltSignedAngle);
//         var verticalTiltFactor =
//             verticalTiltAngle / 90f - 1f; // 1 when upright, 0 when head is tilted 90 degrees on X-axis.
//
//         // Calculate horizontal tilt using left and right eye outer points.
//         var eyeDir = (rightEye - leftEye).normalized;
//         var rightVector = Vector3.right;
//         var horizontalTiltAngle = Vector3.Angle(eyeDir, rightVector);
//         var horizontalTiltSignedAngle = Vector3.SignedAngle(eyeDir, rightVector, Vector3.up);
//         var horizontalSign = Mathf.Sign(horizontalTiltSignedAngle);
//         var horizontalTiltFactor =
//             horizontalTiltAngle / 90f - 1; // 1 when level, 0 when head is tilted 90 degrees on Y-axis.
//
//         // print($"{sign}, {verticalTiltFactor}, {horizontalTiltFactor}");
//
//         return (headOffsetVerticalMultiplier * verticalTiltFactor,
//             headOffsetHorizontalMultiplier * horizontalTiltFactor,
//             -horizontalSign, verticalSign);
//     }
//
//     /// <summary>
//     /// Gets a Vector3 dataset representing world space tracking positions from landmark points, for an eye.
//     /// </summary>
//     /// <param name="landmarks">Input landmarks to pass in.</param>
//     /// <param name="indices">Input indices of landmarks.</param>
//     /// <returns>A Vector3 dataset struct  representing world space eye tracking positions.</returns>
//     private DataStructures.EyeLandMarkData GetEyeData(NormalizedLandmark[] landmarks, params int[] indices)
//     {
//         return new DataStructures.EyeLandMarkData
//         {
//             IrisCenter = Utilities.ConvertToVector3(landmarks[indices[0]]),
//             InnerMost = Utilities.ConvertToVector3(landmarks[indices[1]]),
//             OuterMost = Utilities.ConvertToVector3(landmarks[indices[2]]),
//             TopMost = Utilities.ConvertToVector3(landmarks[indices[3]]),
//             BottomMost = Utilities.ConvertToVector3(landmarks[indices[4]])
//         };
//     }
//
//     #endregion Methods
// }

using System.Collections.Generic;
using Arnab.Scripts;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;
using PimDeWitte.UnityMainThreadDispatcher;

public class IrisTracker : MonoBehaviour
{
    #region Fields & References

    public static IrisTracker Instance;

    [SerializeField] private Transform leftEyeBone;

    [SerializeField] private Transform rightEyeBone;

    [SerializeField] [Range(-70, 70)] private int[] leftEyeAngleRangeInOut = {-38, 39};

    [SerializeField] [Range(-70, 70)] private int[] rightEyeAngleRangeInOut = {-39, 38};

    [SerializeField] [Range(-20, 20)] private float leftEyeOffsetFromRightEye = -15f;

    [SerializeField] [Range(-30, 30)] private int[] bothEyeAngleRangeUpDown = {-25, 30};
    
    // Apply a small deadzone
    [SerializeField] [Range(-0.2f, 0.2f)] private float deadzoneInOut = 0.08f;

    [SerializeField] private float angleMultiplierUpDown = 60f;

    [SerializeField] private float angleMultiplierInOut = 78f;

    private Vector3 _noseTopPos;

    private Vector3 _noseBottomPos;

    private const int NoseTopIndex = 6; 
    private const int NoseBottomIndex = 2; 

    private const int LeftIrisIndex = 473; 
    private const int LeftEyeInnerMostIndex = 362; 
    private const int LeftEyeOuterMostIndex = 263; 
    private const int LeftEyeTopMostIndex = 386; 
    private const int LeftEyeBottomMostIndex = 374; 

    private const int RightIrisIndex = 468; 
    private const int RightEyeInnerMostIndex = 133; 
    private const int RightEyeOuterMostIndex = 33; 
    private const int RightEyeTopMostIndex = 159; 
    private const int RightEyeBottomMostIndex = 145; 

    #endregion Fields & References

    #region Methods

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TrackIrisMovement(List<NormalizedLandmarks> faceLandmarks)
    {
        if (faceLandmarks == null || faceLandmarks.Count == 0)
        {
            Debug.Log("No face landmarks detected.");
            return;
        }

        var landmarks = faceLandmarks[0].landmarks.ToArray();

        _noseTopPos = Utilities.ConvertToVector3(landmarks[NoseTopIndex]);
        _noseBottomPos = Utilities.ConvertToVector3(landmarks[NoseBottomIndex]);

        var leftEyeData = GetEyeData(landmarks, LeftIrisIndex, LeftEyeInnerMostIndex, LeftEyeOuterMostIndex,
            LeftEyeTopMostIndex, LeftEyeBottomMostIndex);

        var rightEyeData = GetEyeData(landmarks, RightIrisIndex, RightEyeInnerMostIndex, RightEyeOuterMostIndex,
            RightEyeTopMostIndex, RightEyeBottomMostIndex);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ApplyEyeRotations(leftEyeData, rightEyeData, _noseTopPos, _noseBottomPos);
        });
    }

    private void ApplyEyeRotations(DataStructures.EyeLandMarkData leftEyeData, DataStructures.EyeLandMarkData rightEyeData,
        Vector3 noseTopPos, Vector3 noseBottomPos)
    {
        var (_, _, horizontalSign, _) = CalculateTiltFactors(noseTopPos, noseBottomPos,
            leftEyeData.OuterMost, rightEyeData.OuterMost);

        var lookingRightSide = horizontalSign > 0;

        var trackedEye = lookingRightSide ? leftEyeBone : rightEyeBone;
        var trackedEyeData = lookingRightSide ? leftEyeData : rightEyeData;
        var trackedEyeAngleMaxRange = lookingRightSide ? leftEyeAngleRangeInOut : rightEyeAngleRangeInOut;

        var followingEye = lookingRightSide ? rightEyeBone : leftEyeBone;
        var followingEyeAngleMaxRange = lookingRightSide ? rightEyeAngleRangeInOut : leftEyeAngleRangeInOut;

        Vector2 irisCenter2D = new Vector2(trackedEyeData.IrisCenter.x, trackedEyeData.IrisCenter.z);
        Vector2 innerPoint2D = new Vector2(trackedEyeData.InnerMost.x, trackedEyeData.InnerMost.z);
        Vector2 outerPoint2D = new Vector2(trackedEyeData.OuterMost.x, trackedEyeData.OuterMost.z);

        float headYRotation = transform.localRotation.eulerAngles.y;
        headYRotation = (headYRotation > 180) ? headYRotation - 360 : headYRotation;

        float trackedEyeAngle = GetIrisBasedAngle(irisCenter2D, innerPoint2D, outerPoint2D, lookingRightSide ? -1f : 1f, headYRotation);

        // Calculate half offset
        float halfOffset = leftEyeOffsetFromRightEye / 2f;

        // Apply rotation with half offset to tracked eye
        ApplyEyeRotation(trackedEye, trackedEyeAngle - (lookingRightSide ? halfOffset : -halfOffset), trackedEyeAngleMaxRange);
    
        // Calculate following eye angle with full offset
        float followingEyeAngle = trackedEyeAngle + (lookingRightSide ? leftEyeOffsetFromRightEye : -leftEyeOffsetFromRightEye);
    
        // Apply rotation with half offset to following eye
        ApplyEyeRotation(followingEye, followingEyeAngle - (lookingRightSide ? halfOffset : -halfOffset), followingEyeAngleMaxRange);
    }

    // private Vector3 CalculateInOutEyeRotation(DataStructures.EyeLandMarkData eyeData, float yAngleOffset, float dirSign,
    //     float horizontalTiltFactor, int[] minMaxAngleRange)
    // {
    //     var inOutAngle = Mathf.Clamp(GetIrisBasedAngle(eyeData.IrisCenter, eyeData.InnerMost,
    //         eyeData.OuterMost, dirSign, angleMultiplierInOut, horizontalTiltFactor), minMaxAngleRange[0], minMaxAngleRange[1]);
    //
    //     return new Vector3(0, inOutAngle + yAngleOffset, 0);
    // }

    private float GetIrisBasedAngle(Vector2 irisCenter, Vector2 innerPoint, Vector2 outerPoint, float dirSign, float headYRotation)
    {
        var distHalf = Vector2.Distance(innerPoint, outerPoint) / 2f;
        var dir = (outerPoint - innerPoint).normalized * dirSign;
        var center = (innerPoint + outerPoint) / 2f;
        var projectedIrisPoint = center + Vector2.Dot(irisCenter - center, dir) * dir;
    
        float normalizedPos = Vector2.Dot(projectedIrisPoint - center, dir) / distHalf;
        
        print(normalizedPos);
    
        if (Mathf.Abs(normalizedPos) < deadzoneInOut)
        {
            return 0f;
        }
        else
        {
            // Adjust the normalized position to account for the dead-zone
            normalizedPos = Mathf.Sign(normalizedPos) * (Mathf.Abs(normalizedPos) - deadzoneInOut) / (1f - deadzoneInOut);
        }
    
        // Adjust for head rotation
        float headRotationFactor = Mathf.Clamp01(Mathf.Abs(headYRotation) / 90f);
        float compensation = headRotationFactor * 0.3f * Mathf.Sign(headYRotation);
        normalizedPos -= compensation;
    
        return Mathf.Clamp(normalizedPos, -1f, 1f);
    }
    
    private void ApplyEyeRotation(Transform eyeBone, float normalizedAngle, int[] angleRange)
    {
        float targetAngle = Mathf.Lerp(angleRange[0], angleRange[1], (normalizedAngle + 1f) / 2f);
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        eyeBone.localRotation = Quaternion.Slerp(eyeBone.localRotation, targetRotation, Time.deltaTime * 15f);
    }

    private (float verticalTiltFactor, float horizontalTiltFactor, float horizontalSign, float verticalSign)
        CalculateTiltFactors(Vector3 noseTop, Vector3 noseBottom, Vector3 leftEye, Vector3 rightEye)
    {
        // Only calculate horizontal tilt for now
        var eyeDir = (rightEye - leftEye).normalized;
        var rightVector = Vector3.right;
        var horizontalTiltSignedAngle = Vector3.SignedAngle(eyeDir, rightVector, Vector3.up);
        var horizontalSign = Mathf.Sign(horizontalTiltSignedAngle);

        // Set vertical factors to neutral values
        return (0f, 1f, -horizontalSign, 1f);
    }

    private DataStructures.EyeLandMarkData GetEyeData(NormalizedLandmark[] landmarks, params int[] indices)
    {
        return new DataStructures.EyeLandMarkData
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