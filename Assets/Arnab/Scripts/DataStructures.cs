using System.Collections.Generic;
using UnityEngine;

namespace Arnab.Scripts
{
    /// <summary>
    /// Project-wide class to maintain various data structures in one place.
    /// </summary>
    public static class DataStructures
    {
        public struct EyeData
        {
            public Vector3 IrisCenter;
            public Vector3 InnerMost;
            public Vector3 OuterMost;
            public Vector3 TopMost;
            public Vector3 BottomMost;
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