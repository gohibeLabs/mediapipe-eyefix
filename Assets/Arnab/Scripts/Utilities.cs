using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

namespace Arnab.Scripts
{
    /// <summary>
    /// Common utility methods usable project wide.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Clamps rotation between two range values on X and Y rotation axes.
        /// Also uses Time.deltaTime to smoothly lerp to target rotation.
        /// Ideally should be called every frame.
        /// </summary>
        /// <param name="target">The target transform.</param>
        /// <param name="targetRotation">Target rotation euler value.</param>
        /// <param name="angleRangeXAxis">A range of minimum (first value) and maximum (second value) angle rotation values on X-axis.</param>
        /// <param name="angleRangeYAxis">A range of minimum (first value) and maximum (second value) angle rotation values on Y-axis.</param>
        public static void ApplyClampedSmoothRotation(Transform target, Vector3 targetRotation, int[] angleRangeXAxis, int[] angleRangeYAxis)
        {
            var clampedRotation = new Vector3(
                Mathf.Clamp(targetRotation.x, angleRangeXAxis[0], angleRangeXAxis[1]),
                Mathf.Clamp(targetRotation.y, angleRangeYAxis[0], angleRangeYAxis[1]), 0);
            target.localRotation = Quaternion.Lerp(target.localRotation, Quaternion.Euler(clampedRotation), Time.deltaTime * 10);
        }
        
        /// <summary>
        /// Converts a normalized landmark to Vector3.
        /// </summary>
        /// <param name="landmark">Input normalized landmark.</param>
        /// <returns>Output Vector3.</returns>
        public static Vector3 ConvertToVector3(NormalizedLandmark landmark) => new(landmark.x, landmark.y, landmark.z);
    }
}