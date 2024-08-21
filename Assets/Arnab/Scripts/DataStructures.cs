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
            public EyeLandMarkData LeftIrisData;
            public EyeLandMarkData RightIrisData;
        }
        
        public struct EyeLandMarkData
        {
            public Transform IrisCenter;
            public Transform InnerMost;
            public Transform OuterMost;
            public Transform[] TopMostPoints;
            public Transform[] BottomMostPoints;
        }
    }
}