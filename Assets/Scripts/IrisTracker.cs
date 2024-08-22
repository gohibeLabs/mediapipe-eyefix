// using System.Collections.Generic;
// using Arnab.Scripts;
// using UnityEngine;
// using Mediapipe.Tasks.Components.Containers;
// using PimDeWitte.UnityMainThreadDispatcher;
//
// /// <summary>
// /// OLD IRIS-FIX 3D points based implementation. This class tracks iris movement and applies rotations to eyeballs accordingly.
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
using System.Linq;
using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Screen;

/// <summary>
/// This class tracks iris movement and applies rotations to eyeballs accordingly.
/// The final approach used here is to track 2D annotation points in order to
/// accurately map eye rotations independent of facial rotations.
/// </summary>
public class IrisTracker : MonoBehaviour
{
    #region Fields & References
    
    [Header("References")] [Space]
    
    [SerializeField] private Transform leftEyeBone;
    [SerializeField] private Transform rightEyeBone;

    [Header("Parameters")] [Space]
    
    [SerializeField] private bool showDebugVisualizePoints = true;
    
    [SerializeField] private string annotatableScreenName = "Annotatable Screen";
    [SerializeField] private string faceLandmarkListAnnotationName = "FaceLandmarkList Annotation";
    [SerializeField] private string leftIrisLandmarkListAnnotation = "Left IrisLandmarkList Annotation";
    [SerializeField] private string rightIrisLandmarkListAnnotation = "Right IrisLandmarkList Annotation";
    
    [SerializeField] private int leftEyeInnerMostIndex = 133;
    [SerializeField] private int leftEyeOuterMostIndex = 33;
    [SerializeField] private int[] leftEyeTopMostIndices = {107, 66, 105, 63};
    [SerializeField] private int[] leftEyeBottomMostIndices = {231, 230, 229, 228};

    [SerializeField] private int rightEyeInnerMostIndex = 362;
    [SerializeField] private int rightEyeOuterMostIndex = 263;
    [SerializeField] private int[] rightEyeTopMostIndices = {336, 296, 334, 293};
    [SerializeField] private int[] rightEyeBottomMostIndices = {451, 450, 449, 448};
    
    [SerializeField] [Range(-70, 70)] private int[] eyeAngleRangeInOut = {60, -60};
    [SerializeField] [Range(-90, 90)] private int[] eyeAngleRangeUpDown = {-90, 30};
    [SerializeField] [Range(0f, 1f)] private float eyeCenterKappaNormalizedOffset = 0.14f;
    
    [SerializeField] private float trackingResponsivenessMultiplier = 35f;
    [SerializeField] private float angleMultiplierUpDown = 1.6f;
    [SerializeField] private float angleMultiplierInOut = 1.6f;
    
    [Header("Debugging")] [Space]
    
    [SerializeField] private Transform trackedLeftIrisCenter;
    [SerializeField] private Transform trackedLeftEyeInner;
    [SerializeField] private Transform trackedLeftEyeOuter;
    [SerializeField] private Transform[] trackedLeftEyeTopPoints;
    [SerializeField] private Transform[] trackedLeftEyeBottomPoints;
    
    [SerializeField] private Transform trackedRightIrisCenter;
    [SerializeField] private Transform trackedRightEyeInner;
    [SerializeField] private Transform trackedRightEyeOuter;
    [SerializeField] private Transform[] trackedRightEyeTopPoints;
    [SerializeField] private Transform[] trackedRightEyeBottomPoints;

    private readonly List<MeshRenderer> _debugTrackedPoints = new();
    
    //////////////////////// Private variables ////////////////////////////
    
    private EyeAnnotationSet FaceIris2DPoints { get; set; }
    
    private Transform _facePoints2DTransform;
    private Transform _leftIrisPoints2DTransform;
    private Transform _rightIrisPoints2DTransform;
    
    private Transform _currentLeftEyeNearestTopPoint;
    private Transform _currentLeftEyeNearestBottomPoint;
    
    private Transform _currentRightEyeNearestTopPoint;
    private Transform _currentRightEyeNearestBottomPoint;
    
    #endregion Fields & References

    #region Unity Events

    private void Awake()
    {
        // Make tracking as smooth as screen refresh rate.
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }

    private void Update()
    {
        // If no facial annotation transform was found, do not attempt updating eye rotations and attempt to initialize transforms first.
        if (_facePoints2DTransform == null)
        {
            InitializeIris2DData();
            return;
        }
        
        // If no annotation points are generated for any necessary transform, ignore sync.
        if (_facePoints2DTransform.childCount < 0) return;
        if (_leftIrisPoints2DTransform.childCount < 0) return;
        if (_rightIrisPoints2DTransform.childCount < 0) return;
        
        // Visualize annotations and webcam view if debugging is on.
        if (showDebugVisualizePoints)
            DebugVisualizePoints(FaceIris2DPoints);

        // Sync iris rotations in real-time.
        SyncIrisMovement(FaceIris2DPoints);
    }
    
    #endregion Unity Events
    
    #region Logic Methods

    /// <summary>
    /// Initialize tracking data sets.
    /// Retrieves necessary transforms to acquire MediaPipe's annotation points.
    /// </summary>
    private void InitializeIris2DData()
    {
        // Find necessary MediaPipe gameObjects by name.
        // NOTE: These serialized names of gameObjects as noted down as of this time of writing the code,
        // future MediaPipe package upgrades might change hierarchical relations or names of these gameObjects.
        var webCamView = GameObject.Find(annotatableScreenName);
        var facePoints = GameObject.Find(faceLandmarkListAnnotationName);
        var leftIrisPoints = GameObject.Find(leftIrisLandmarkListAnnotation);
        var rightIrisPoints = GameObject.Find(rightIrisLandmarkListAnnotation);
        
        // If any of the needed gameObjects are missing, abort initialization.
        if (!webCamView || !facePoints || !leftIrisPoints || !rightIrisPoints)
            return;
        
        // Get "Point List Annotation", residing under "FaceLandmarkListWithIris Annotation" prefab -> As first child under "FaceLandmarkList Annotation".
        // NOTE: These serialized names of gameObjects as noted down as of this time of writing the code,
        // future MediaPipe package upgrades might change hierarchical relations or names of these gameObjects.
        _facePoints2DTransform = facePoints.transform.GetChild(0);
        
        // Get "Point List Annotation", residing under "FaceLandmarkListWithIris Annotation" prefab -> As first child under "Left IrisLandmarkList Annotation".
        _leftIrisPoints2DTransform = leftIrisPoints.transform.GetChild(0);
        
        // Get "Circle Annotation", residing under "FaceLandmarkListWithIris Annotation" prefab -> As first child under "Left IrisLandmarkList Annotation".
        var leftIrisCircle = leftIrisPoints.transform.GetChild(1);
        
        // Get "Point List Annotation", residing under "FaceLandmarkListWithIris Annotation" prefab -> As first child under "Right IrisLandmarkList Annotation".
        _rightIrisPoints2DTransform = rightIrisPoints.transform.GetChild(0);
        
        // Get "Circle Annotation", residing under "FaceLandmarkListWithIris Annotation" prefab -> As first child under "Right IrisLandmarkList Annotation".
        var rightIrisCircle = rightIrisPoints.transform.GetChild(1);

        foreach (Transform point in _facePoints2DTransform)
            HidePoints(point);
        foreach (Transform point in _leftIrisPoints2DTransform)
            HidePoints(point);
        foreach (Transform point in _rightIrisPoints2DTransform)
            HidePoints(point);

        SetDebugVisualCircle(leftIrisCircle, showDebugVisualizePoints);
        SetDebugVisualCircle(rightIrisCircle, showDebugVisualizePoints);

        FaceIris2DPoints = new EyeAnnotationSet
        {
            LeftEyeData = default,
            RightEyeData = default
        };

        if (webCamView)
            webCamView.GetComponent<RawImage>().enabled = showDebugVisualizePoints;
        
        // Finally set all 2D annotation transforms to global variables once.
        SetFacial2DPoints();
        
        return;

        void HidePoints(Transform point)
        {
            point.GetComponent<MeshRenderer>().enabled = false;
            point.GetComponent<Collider>().enabled = false;
        }
        
        void SetDebugVisualCircle(Transform circle, bool show)
        {
            var lineRend = circle.GetComponent<LineRenderer>();
            lineRend.enabled = show;
            lineRend.startWidth = lineRend.endWidth = 0.3f;
            circle.GetComponent<CircleAnnotation>().enabled = show;
        }
    }
    
    /// <summary>
    /// Sets the actual Facial 2D annotation points data for both eyes.
    /// </summary>
    private void SetFacial2DPoints()
    {
        var leftEyeTopPoints = leftEyeTopMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
        var leftEyeBottomPoints = leftEyeBottomMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
            
        var rightEyeTopPoints = rightEyeTopMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
        var rightEyeBottomPoints = rightEyeBottomMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();

        var leftIris2DData = new EyeLandMarkAnnotations
        {
            IrisCenter = _leftIrisPoints2DTransform.GetChild(0),
            InnerMost = _facePoints2DTransform.GetChild(leftEyeInnerMostIndex),
            OuterMost = _facePoints2DTransform.GetChild(leftEyeOuterMostIndex),
            TopMostPoints = leftEyeTopPoints.ToArray(),
            BottomMostPoints = leftEyeBottomPoints.ToArray()
        };
            
        var rightIris2DData = new EyeLandMarkAnnotations
        {
            IrisCenter = _rightIrisPoints2DTransform.GetChild(0),
            InnerMost = _facePoints2DTransform.GetChild(rightEyeInnerMostIndex),
            OuterMost = _facePoints2DTransform.GetChild(rightEyeOuterMostIndex),
            TopMostPoints = rightEyeTopPoints.ToArray(),
            BottomMostPoints = rightEyeBottomPoints.ToArray()
        };

        // Important: Flip the right and left eye data passed as our eyes from webcam are flipped w.r.t. avatar's eyes.
        FaceIris2DPoints = new EyeAnnotationSet
        {
            LeftEyeData = rightIris2DData,
            RightEyeData = leftIris2DData
        };
    }

    /// <summary>
    /// Debug & visualize all tracked facial points for iris rotation.
    /// Should not be called in production release to save performance.
    /// </summary>
    /// <param name="face2DPoints">Input 2D facial points data object.</param>
    private void DebugVisualizePoints(EyeAnnotationSet face2DPoints)
    {
        // Get eye data.
        var leftEyeData = face2DPoints.LeftEyeData;
        var rightEyeData = face2DPoints.RightEyeData;

        // If debug point renderers list isn't populated, update it.
        if (_debugTrackedPoints.Count < 1)
        {
            _debugTrackedPoints.Add(leftEyeData.IrisCenter.GetComponent<MeshRenderer>());
            _debugTrackedPoints.Add(leftEyeData.InnerMost.GetComponent<MeshRenderer>());
            _debugTrackedPoints.Add(leftEyeData.OuterMost.GetComponent<MeshRenderer>());
            foreach (var point in leftEyeData.TopMostPoints)
                _debugTrackedPoints.Add(point.GetComponent<MeshRenderer>());
            foreach (var point in leftEyeData.BottomMostPoints)
                _debugTrackedPoints.Add(point.GetComponent<MeshRenderer>());
            
            _debugTrackedPoints.Add(rightEyeData.IrisCenter.GetComponent<MeshRenderer>());
            _debugTrackedPoints.Add(rightEyeData.InnerMost.GetComponent<MeshRenderer>());
            _debugTrackedPoints.Add(rightEyeData.OuterMost.GetComponent<MeshRenderer>());
            foreach (var point in rightEyeData.TopMostPoints)
                _debugTrackedPoints.Add(point.GetComponent<MeshRenderer>());
            foreach (var point in rightEyeData.BottomMostPoints)
                _debugTrackedPoints.Add(point.GetComponent<MeshRenderer>());
            
            // Enable the rendering of only those points which are tracked.
            foreach (var pointRend in 
                     _debugTrackedPoints.Where(pointRend => !pointRend.enabled))
                pointRend.enabled = true;
        }
    }

    /// <summary>
    /// Retrieves the closest vertical (Top and bottom) points for both eyes.
    /// </summary>
    private void GetNearestVerticalPoints()
    {
        if (trackedLeftIrisCenter)
        {
            _currentLeftEyeNearestTopPoint = GetClosest(trackedLeftIrisCenter.position, trackedLeftEyeTopPoints);
            _currentLeftEyeNearestBottomPoint = GetClosest(trackedLeftIrisCenter.position, trackedLeftEyeBottomPoints);
        }

        if (trackedRightIrisCenter)
        {
            _currentRightEyeNearestTopPoint = GetClosest(trackedRightIrisCenter.position, trackedRightEyeTopPoints);
            _currentRightEyeNearestBottomPoint = GetClosest(trackedRightIrisCenter.position, trackedRightEyeBottomPoints);
        }
    }

    /// <summary>
    /// Gets the closes point to the start position passed.
    /// Uses squared magnitude for distance check for efficiency.
    /// </summary>
    /// <param name="startPosition">The position of point in question.</param>
    /// <param name="points">The set of input points to check distance with.</param>
    /// <returns>The nearest point to passed in start point position.</returns>
    private static Transform GetClosest(Vector3 startPosition, Transform[] points)
    {
        Transform bestTarget = null;
        
        var closestDistanceSqr = Mathf.Infinity;

        foreach (var potentialTarget in points)
        {
            var directionToTarget = potentialTarget.transform.position - startPosition;

            var dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }
    
    /// <summary>
    /// Track iris movement and apply rotations to eyes from 2D facial tracked points.
    /// </summary>
    /// <param name="face2DPoints">Input 2D facial points data object.</param>
    private void SyncIrisMovement(EyeAnnotationSet face2DPoints)
    {
        var leftEyeData = face2DPoints.LeftEyeData;
        var rightEyeData = face2DPoints.RightEyeData;
        
        trackedLeftIrisCenter = leftEyeData.IrisCenter;
        trackedLeftEyeInner = leftEyeData.InnerMost;
        trackedLeftEyeOuter = leftEyeData.OuterMost;
        trackedLeftEyeTopPoints = leftEyeData.TopMostPoints;
        trackedLeftEyeBottomPoints = leftEyeData.BottomMostPoints;
        
        trackedRightIrisCenter = rightEyeData.IrisCenter;
        trackedRightEyeInner = rightEyeData.InnerMost;
        trackedRightEyeOuter = rightEyeData.OuterMost;
        trackedRightEyeTopPoints = rightEyeData.TopMostPoints;
        trackedRightEyeBottomPoints = rightEyeData.BottomMostPoints;

        GetNearestVerticalPoints();

        var leftEyeValue = GetIrisBasedNormalizedValue(trackedLeftIrisCenter.position, trackedLeftEyeInner.position, trackedLeftEyeOuter.position,
            _currentLeftEyeNearestTopPoint.position, _currentLeftEyeNearestBottomPoint.position);
        var rightEyeValue = GetIrisBasedNormalizedValue(trackedRightIrisCenter.position, trackedRightEyeOuter.position, trackedRightEyeInner.position,
            _currentRightEyeNearestTopPoint.position, _currentRightEyeNearestBottomPoint.position);

        var averageEyesX = Mathf.Abs(leftEyeValue.x + Mathf.Abs(rightEyeValue.x)) / 2f;
        var eyesOffsetPower = 1f - Mathf.Abs(averageEyesX);
        var eyeOffsetsHalf = eyeCenterKappaNormalizedOffset * eyesOffsetPower;
        
        leftEyeValue.x = (Mathf.Clamp(leftEyeValue.x, -1f, 1f) - eyeOffsetsHalf) * angleMultiplierInOut;
        rightEyeValue.x = (Mathf.Clamp(rightEyeValue.x, -1f, 1f) + eyeOffsetsHalf) * angleMultiplierInOut;

        leftEyeValue.y *= -angleMultiplierUpDown;
        rightEyeValue.y = leftEyeValue.y;

        // Apply rotation with half offset to left eye
        ApplyEyeRotation(leftEyeBone, leftEyeValue, eyeAngleRangeInOut, eyeAngleRangeUpDown);

        // Apply rotation with half offset to right eye
        ApplyEyeRotation(rightEyeBone, rightEyeValue, eyeAngleRangeInOut, eyeAngleRangeUpDown);
    }
    
    /// <summary>
    /// Gets the normalized value between -0.5 and 0.5 for the iris center point relative to inner and outer eye points.
    /// </summary>
    /// <param name="irisCenter">The iris center annotation point world location in 2D.</param>
    /// <param name="innerPoint">The eye's innermost annotation point world location in 2D.</param>
    /// <param name="outerPoint">The eye's outermost annotation point world location in 2D.</param>
    /// <param name="nearestTopPoint">The eye's nearest top most annotation point world location in 2D.</param>
    /// <param name="nearestBottomPoint">The eye's nearest bottom most annotation point world location in 2D.</param>
    /// <returns></returns>
    private static Vector2 GetIrisBasedNormalizedValue(Vector2 irisCenter, Vector2 innerPoint, Vector2 outerPoint, Vector2 nearestTopPoint, Vector2 nearestBottomPoint)
    {
        // Horizontal normalized value calculation using two extreme points.
        var eyeWidth = Vector2.Distance(innerPoint, outerPoint);
        var eyeHorizontalDir = (outerPoint - innerPoint).normalized;
        var eyeCenter = (innerPoint + outerPoint) / 2f;
        var horizontalOffset = Vector2.Dot(irisCenter - eyeCenter, eyeHorizontalDir);
        var normalizedHorizontal = horizontalOffset / (eyeWidth / 2f);

        // Vertical normalized value calculation using two extreme points.
        var eyeHeight = Vector2.Distance(nearestTopPoint, nearestBottomPoint);
        var eyeVerticalDir = (nearestTopPoint - nearestBottomPoint).normalized;
        eyeCenter = (nearestTopPoint + nearestBottomPoint) / 2f;
        var verticalOffset = Vector2.Dot(irisCenter - eyeCenter, eyeVerticalDir);
        var normalizedVertical = verticalOffset / (eyeHeight / 2f);

        return new Vector2(normalizedHorizontal, normalizedVertical);
    }
    
    /// <summary>
    /// Apply final eye rotations to an eye.
    /// Final angle being set is in degrees.
    /// </summary>
    /// <param name="eyeBone">The eye bone to set rotation on.</param>
    /// <param name="normalizedValue">The input normalized value</param>
    /// <param name="angleRangeHorizontal">The range of rotation for horizontal eye rotation.</param>
    /// <param name="angleRangeVertical">The range of rotation for vertical eye rotation.</param>
    private void ApplyEyeRotation(Transform eyeBone, Vector2 normalizedValue, int[] angleRangeHorizontal, int[] angleRangeVertical)
    {
        // Lerp between the two min-max rotation range values depending on input normalized value for both axes.
        var horizontalAngle = Mathf.Lerp(angleRangeHorizontal[0], angleRangeHorizontal[1], (normalizedValue.x + 1f) / 2f);
        var verticalAngle = Mathf.Lerp(angleRangeVertical[0], angleRangeVertical[1], (normalizedValue.y + 1f) / 2f);
    
        // Smoothly set target rotation on eye bone.
        var targetRotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        eyeBone.localRotation = Quaternion.Slerp(eyeBone.localRotation, targetRotation, Time.deltaTime * trackingResponsivenessMultiplier);
    }

    #endregion Logic Methods
    
    #region Data Structures

    /// <summary>
    /// A struct containing all 2D annotation transforms for both eyes.
    /// </summary>
    private struct EyeAnnotationSet
    {
        public EyeLandMarkAnnotations LeftEyeData;
        public EyeLandMarkAnnotations RightEyeData;
    }

    /// <summary>
    /// A struct containing the 2D annotation transforms found on the webcam view raw-image.
    /// </summary>
    private struct EyeLandMarkAnnotations
    {
        public Transform IrisCenter;
        public Transform InnerMost;
        public Transform OuterMost;
        public Transform[] TopMostPoints;
        public Transform[] BottomMostPoints;
    }

    #endregion Data Structures
}