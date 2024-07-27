using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;
using PimDeWitte.UnityMainThreadDispatcher;

public class IrisTracker : MonoBehaviour
{
    [SerializeField] private Transform leftEyeBone;
    [SerializeField] private Transform rightEyeBone;

    // Indices for landmarks
    private static readonly int leftIrisIndex = 473; // Center of the left iris
    private static readonly int leftEyeInnermostIndex = 463; // Inner corner of the left eye
    private static readonly int leftEyeOutermostIndex = 263; // Outer corner of the left eye

    private static readonly int rightIrisIndex = 468; // Center of the right iris
    private static readonly int rightEyeInnermostIndex = 133; // Inner corner of the right eye
    private static readonly int rightEyeOutermostIndex = 33; // Outer corner of the right eye

    private Quaternion initialLeftEyeRotation;
    private Quaternion initialRightEyeRotation;

    public static IrisTracker Instance;

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
        initialLeftEyeRotation = leftEyeBone.localRotation;
        initialRightEyeRotation = rightEyeBone.localRotation;
    }

    public void TrackIrisMovement(List<NormalizedLandmarks> faceLandmarks)
    {
        if (faceLandmarks == null || faceLandmarks.Count == 0)
        {
            Debug.Log("No face landmarks detected.");
            return;
        }

        Vector3 leftIrisCenter = ConvertToVector3(faceLandmarks[0].landmarks[leftIrisIndex]);
        Vector3 leftEyeInnermostPoint = ConvertToVector3(faceLandmarks[0].landmarks[leftEyeInnermostIndex]);
        Vector3 leftEyeOutermostPoint = ConvertToVector3(faceLandmarks[0].landmarks[leftEyeOutermostIndex]);

        Vector3 rightIrisCenter = ConvertToVector3(faceLandmarks[0].landmarks[rightIrisIndex]);
        Vector3 rightEyeInnermostPoint = ConvertToVector3(faceLandmarks[0].landmarks[rightEyeInnermostIndex]);
        Vector3 rightEyeOutermostPoint = ConvertToVector3(faceLandmarks[0].landmarks[rightEyeOutermostIndex]);

        Debug.Log($"Left Iris Center: {leftIrisCenter}");
        Debug.Log($"Left Eye Innermost Point: {leftEyeInnermostPoint}");
        Debug.Log($"Left Eye Outermost Point: {leftEyeOutermostPoint}");

        Debug.Log($"Right Iris Center: {rightIrisCenter}");
        Debug.Log($"Right Eye Innermost Point: {rightEyeInnermostPoint}");
        Debug.Log($"Right Eye Outermost Point: {rightEyeOutermostPoint}");

        ApplyYRotation(leftEyeBone, leftIrisCenter, leftEyeInnermostPoint, leftEyeOutermostPoint, initialLeftEyeRotation);
        ApplyYRotation1(rightEyeBone, rightIrisCenter, rightEyeInnermostPoint, rightEyeOutermostPoint, initialRightEyeRotation);
    }

    private Vector3 ConvertToVector3(NormalizedLandmark landmark)
    {
        return new Vector3(landmark.x, landmark.y, landmark.z);
    }

    private void ApplyYRotation(Transform eyeBone, Vector3 irisCenter, Vector3 innermostPoint, Vector3 outermostPoint, Quaternion initialEyeRotation)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Calculate the direction from innermost to outermost point
            Vector3 eyeRangeDirection = (outermostPoint - innermostPoint).normalized;

            // Calculate the direction from innermost point to iris center
            Vector3 irisDirection = (irisCenter - innermostPoint).normalized;

            // Calculate the angle between the two directions
            float angle = Vector3.SignedAngle(eyeRangeDirection, irisDirection, Vector3.up);

            // Assuming the max rotation for the eye bone is 30 degrees
            float maxRotationAngle = 30.0f;

            // Calculate the target rotation angle based on the calculated angle
            float targetRotationAngle = Mathf.Clamp(angle - 23.0f, -30, 20);

            // Create the target rotation
            Quaternion targetRotation = Quaternion.Euler(initialEyeRotation.eulerAngles.x, initialEyeRotation.eulerAngles.y + targetRotationAngle, initialEyeRotation.eulerAngles.z);

            // Apply the rotation to the eye bone
            eyeBone.localRotation = targetRotation;

            // Log the final applied rotation
            Debug.Log($"Calculated Angle: {angle}");
            Debug.Log($"Target Rotation Angle: {targetRotationAngle}");
            Debug.Log($"Final Rotation for {eyeBone.name}: {eyeBone.localEulerAngles}");
        });
    }

    private void ApplyYRotation1(Transform eyeBone, Vector3 irisCenter, Vector3 innermostPoint, Vector3 outermostPoint, Quaternion initialEyeRotation)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Calculate the direction from innermost to outermost point
            Vector3 eyeRangeDirection = (outermostPoint - innermostPoint).normalized;

            // Calculate the direction from innermost point to iris center
            Vector3 irisDirection = (irisCenter - innermostPoint).normalized;

            // Calculate the angle between the two directions
            float angle = Vector3.SignedAngle(eyeRangeDirection, irisDirection, Vector3.up);

            // Assuming the max rotation for the eye bone is 30 degrees
            float maxRotationAngle = 30.0f;

            // Calculate the target rotation angle based on the calculated angle
            float targetRotationAngle = Mathf.Clamp(angle + 20.0f, -18, 30);

            // Create the target rotation
            Quaternion targetRotation = Quaternion.Euler(initialEyeRotation.eulerAngles.x, initialEyeRotation.eulerAngles.y + targetRotationAngle, initialEyeRotation.eulerAngles.z);

            // Apply the rotation to the eye bone
            eyeBone.localRotation = targetRotation;

            // Log the final applied rotation
            Debug.Log($"Calculated Angle: {angle}");
            Debug.Log($"Target Rotation Angle: {targetRotationAngle}");
            Debug.Log($"Final Rotation for {eyeBone.name}: {eyeBone.localEulerAngles}");
        });
    }

}
