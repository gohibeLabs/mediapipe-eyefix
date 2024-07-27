using System.Collections.Generic;
using UnityEngine;

public class IrisMovement : MonoBehaviour
{
    public GameObject leftIris;
    public GameObject rightIris;

    // Indices for eye and iris landmarks
    private readonly int[] leftEyeIndices = { 33, 133, 160, 144, 153, 362, 263, 387, 373, 380 };
    private readonly int[] rightEyeIndices = { 362, 263, 387, 373, 380 };
    private readonly int[] leftIrisIndices = { 468, 469, 470, 471 };
    private readonly int[] rightIrisIndices = { 473, 474, 475, 476 };

    private List<Vector3> landmarks;

    // Placeholder method to simulate getting the landmark data from MediaPipe
    private List<Vector3> GetFaceLandmarksFromMediaPipe()
    {
        // Replace this with your actual method to retrieve the landmark data
        return new List<Vector3>();
    }

    void Update()
    {
        // Get the landmark data
        landmarks = GetFaceLandmarksFromMediaPipe();

        // Ensure landmarks are available
        if (landmarks.Count == 0) return;

        // Calculate and apply the iris positions
        Vector3 leftIrisPosition = CalculateIrisPosition(leftEyeIndices, leftIrisIndices);
        Vector3 rightIrisPosition = CalculateIrisPosition(rightEyeIndices, rightIrisIndices);

        leftIris.transform.localPosition = leftIrisPosition;
        rightIris.transform.localPosition = rightIrisPosition;
    }

    Vector3 CalculateIrisPosition(int[] eyeIndices, int[] irisIndices)
    {
        // Calculate the eye boundary center
        Vector3 eyeCenter = Vector3.zero;
        foreach (int index in eyeIndices)
        {
            eyeCenter += landmarks[index];
        }
        eyeCenter /= eyeIndices.Length;

        // Calculate the iris position
        Vector3 irisPosition = Vector3.zero;
        foreach (int index in irisIndices)
        {
            irisPosition += landmarks[index];
        }
        irisPosition /= irisIndices.Length;

        // Offset iris position relative to the eye center
        Vector3 relativeIrisPosition = irisPosition - eyeCenter;

        // Scale the relative iris position for better visual representation
        relativeIrisPosition *= 0.1f; // Adjust this value as needed

        return relativeIrisPosition;
    }
}
