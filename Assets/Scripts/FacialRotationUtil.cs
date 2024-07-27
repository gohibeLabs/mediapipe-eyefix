using UnityEngine;
using System.Collections.Generic;

public class FacialRotationUtil : MonoBehaviour
{
    /// <summary>
    /// Converts a list of MediaPipe transformation matrices to a list of Quaternions.
    /// </summary>
    /// <param name="facialTransformationMatrices">A list of 4x4 transformation matrices from MediaPipe.</param>
    /// <returns>A list of Quaternions representing the rotations.</returns>
    public static List<Quaternion> GetRotationsFromMatrices(List<Matrix4x4> facialTransformationMatrices)
    {
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (Matrix4x4 matrix in facialTransformationMatrices)
        {
            // Log the input matrix
            //Debug.Log("Input Matrix: " + matrix.ToString());

            // Extract the rotation from the matrix
            Quaternion rotation = ExtractRotation(matrix);

            // Log the extracted rotation
            //Debug.Log("Extracted Rotation: " + rotation.eulerAngles.ToString());

            rotations.Add(rotation);
        }

        return rotations;
    }

    /// <summary>
    /// Extracts the rotation quaternion from a transformation matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The rotation quaternion.</returns>
    private static Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }
}
