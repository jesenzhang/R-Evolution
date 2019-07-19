using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneSeparate
{
    [System.Serializable]
    public class SceneSeparateData : ScriptableObject
    {
        [SerializeField]
        public string sceneName;
        [SerializeField]
        public Bounds bounds;
        [SerializeField]
        public bool asyn=true;
        [SerializeField]
        public int treeDepth=5;
        [SerializeField]
        public TreeType treeType = TreeType.LinearQuadTree;
        [SerializeField]
        public int maxCreateCount = 25;
        [SerializeField]
        public int minCreateCount = 15;
        [SerializeField]
        public int maxRefreshTime =1;
        [SerializeField]
        public int maxDestroyTime =5;
        [SerializeField]
        public List<SceneNode> nodes;
    }
}