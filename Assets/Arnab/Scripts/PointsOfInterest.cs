using System.Collections.Generic;
using System.Linq;
using Mediapipe.Unity;
using UnityEngine;

namespace Arnab.Scripts
{
    public class PointsOfInterest : MonoBehaviour
    {
        [SerializeField] private PointListAnnotation facePoints2D;
        [SerializeField] private PointListAnnotation leftIrisPoints2D;
        [SerializeField] private PointListAnnotation rightIrisPoints2D;
        
        private const int NoseTopIndex = 6;
        private const int NoseBottomIndex = 2;

        private const int LeftEyeInnerMostIndex = 133;
        private const int LeftEyeOuterMostIndex = 33;
        // private const int LeftEyeTopMostIndex = 159;
        // private const int LeftEyeBottomMostIndex = 145;
        private readonly int[] _leftEyeTopMostIndices = {107, 66, 105, 63};
        private readonly int[] _leftEyeBottomMostIndices = {231, 230, 229, 228};

        private const int RightEyeInnerMostIndex = 362;
        private const int RightEyeOuterMostIndex = 263;
        // private const int RightEyeTopMostIndex = 386;
        // private const int RightEyeBottomMostIndex = 374;
        private readonly int[] _rightEyeTopMostIndices = {336, 296, 334, 293};
        private readonly int[] _rightEyeBottomMostIndices = {451, 450, 449, 448};

        private Transform _facePoints2DTransform;
        private Transform _leftIrisPoints2DTransform;
        private Transform _rightIrisPoints2DTransform;

        private DataStructures.FacialIrisTrackPointIndices FaceIris2DPoints { get; set; }

        private void Awake()
        {
            _facePoints2DTransform = facePoints2D.transform;
            _leftIrisPoints2DTransform = leftIrisPoints2D.transform;
            _rightIrisPoints2DTransform = rightIrisPoints2D.transform;

            FaceIris2DPoints = new DataStructures.FacialIrisTrackPointIndices
            {
                FaceNoseTopPos = default,
                FaceNoseBottomPos = default,
                LeftIrisData = default,
                RightIrisData = default
            };
        }

        private void Update()
        {
            if (_facePoints2DTransform.childCount < 0) return;
            if (_leftIrisPoints2DTransform.childCount < 0) return;
            if (_rightIrisPoints2DTransform.childCount < 0) return;

            SetFacial2DPoints();

            IrisTracker.Instance.TrackIrisMovement(FaceIris2DPoints);
        }

        private void SetFacial2DPoints()
        {
            var leftEyeTopPoints = _leftEyeTopMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
            var leftEyeBottomPoints = _leftEyeBottomMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
            
            var rightEyeTopPoints = _rightEyeTopMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();
            var rightEyeBottomPoints = _rightEyeBottomMostIndices.Select(point => _facePoints2DTransform.GetChild(point)).ToList();

            var leftIris2DData = new DataStructures.EyeLandMarkData
            {
                IrisCenter = _leftIrisPoints2DTransform.GetChild(0),
                InnerMost = _facePoints2DTransform.GetChild(LeftEyeInnerMostIndex),
                OuterMost = _facePoints2DTransform.GetChild(LeftEyeOuterMostIndex),
                // TopMost = _facePoints2DTransform.GetChild(LeftEyeTopMostIndex),
                // BottomMost = _facePoints2DTransform.GetChild(LeftEyeBottomMostIndex),
                TopMostPoints = leftEyeTopPoints.ToArray(),
                BottomMostPoints = leftEyeBottomPoints.ToArray()
            };
            
            var rightIris2DData = new DataStructures.EyeLandMarkData
            {
                IrisCenter = _rightIrisPoints2DTransform.GetChild(0),
                InnerMost = _facePoints2DTransform.GetChild(RightEyeInnerMostIndex),
                OuterMost = _facePoints2DTransform.GetChild(RightEyeOuterMostIndex),
                // TopMost = _facePoints2DTransform.GetChild(RightEyeTopMostIndex),
                // BottomMost = _facePoints2DTransform.GetChild(RightEyeBottomMostIndex),
                TopMostPoints = rightEyeTopPoints.ToArray(),
                BottomMostPoints = rightEyeBottomPoints.ToArray()
            };

            FaceIris2DPoints = new DataStructures.FacialIrisTrackPointIndices
            {
                FaceNoseTopPos = _facePoints2DTransform.GetChild(NoseTopIndex),
                FaceNoseBottomPos = _facePoints2DTransform.GetChild(NoseBottomIndex),
                LeftIrisData = leftIris2DData,
                RightIrisData = rightIris2DData
            };
        }
    }
}