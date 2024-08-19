using System.Collections.Generic;
using UnityEngine;

namespace Arnab.Scripts
{
    /// <summary>
    /// Project-wide class to maintain various data structures in one place.
    /// </summary>
    public static class DataStructures
    {
        public struct FacialIrisTrackPointIndices
        {
            public Transform FaceNoseTopPos;
            public Transform FaceNoseBottomPos;
            public EyeLandMarkData LeftIrisData;
            public EyeLandMarkData RightIrisData;
        }
        
        public struct EyeLandMarkData
        {
            public Transform IrisCenter;
            public Transform InnerMost;
            public Transform OuterMost;
            // public Transform TopMost;
            // public Transform BottomMost;
            public Transform[] TopMostPoints;
            public Transform[] BottomMostPoints;
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
        public class BsCategoryNames
        {
            public List<string> categoryNames;
        }
    }
}