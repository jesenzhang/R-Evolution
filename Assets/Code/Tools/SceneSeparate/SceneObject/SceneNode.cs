using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SceneSeparate
{
    [System.Serializable]
    public class LightMapData
    {
        [SerializeField]
        public string path;
        [SerializeField]
        public int lightmapIndex;
        [SerializeField]
        public Vector4 lightmapScaleOffset;
        [SerializeField]
        public int realtimeLightmapIndex;
        [SerializeField]
        public Vector4 realtimeLightmapScaleOffset;
        [SerializeField]
        public UnityEngine.Rendering.LightProbeUsage lightProbeUsage;
    }

    /// <summary>
    /// 测试场景物体-实际应用中可以根据需求增加或修改，只需实现ISceneObject接口即可
    /// </summary>
    [System.Serializable]
    public class SceneNode : ISceneObject
    {
        [SerializeField]
        private Bounds m_Bounds;
        [SerializeField]
        private string m_ResPath;
        [SerializeField]
        private Vector3 m_Position;
        [SerializeField]
        private Vector3 m_Rotation;
        [SerializeField]
        private Vector3 m_LocalScale;
        [SerializeField]
        private string m_ScenePath;
        [SerializeField]
        private string m_SceneParent;
        [SerializeField]
        private string m_SceneName;
        [SerializeField]
        private int m_Layer;
        [SerializeField]
        private string m_Tag;
        [SerializeField]
        private int resID;
        
        private GameObject m_LoadedPrefab;
        [SerializeField]
        private List<LightMapData> lightMapDatas;

        public String ScenePath
        {
            get
            {
                return m_ScenePath;
            }

            set
            {
                m_ScenePath = value;
            }
        }

        public Vector3 LocalScale
        {
            get
            {
                return m_LocalScale;
            }

            set
            {
                m_LocalScale = value;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return m_Rotation;
            }

            set
            {
                m_Rotation = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return m_Position;
            }

            set
            {
                m_Position = value;
            }
        }

        public String ResPath
        {
            get
            {
                return m_ResPath;
            }

            set
            {
                m_ResPath = value;
            }
        }

        public Bounds Bounds
        {
            get
            {
                return m_Bounds;
            }

            set
            {
                m_Bounds = value;
            }
        }

        public List<LightMapData> LightMapDatas
        {
            get
            {
                return lightMapDatas;
            }

            set
            {
                lightMapDatas = value;
            }
        }

        public String SceneParent
        {
            get
            {
                return m_SceneParent;
            }

            set
            {
                m_SceneParent = value;
            }
        }

        public String SceneName
        {
            get
            {
                return m_SceneName;
            }

            set
            {
                m_SceneName = value;
            }
        }

        public int Layer { get => m_Layer; set => m_Layer = value; }
        public string Tag { get => m_Tag; set => m_Tag = value; }
        public int ResID { get => resID; set => resID = value; }

        public void OnHide()
        {
            if (m_LoadedPrefab)
            {
                GameObject.Destroy(m_LoadedPrefab);
                m_LoadedPrefab = null;
            }
        }
        void DelegateResourceCallBack(int index, object obj)
        {
            if (obj == null) return;
            m_LoadedPrefab =  (GameObject)obj;
            GameObject parentobj = GameObject.Find(SceneParent);
            m_LoadedPrefab.transform.SetParent(parentobj.transform);
            m_LoadedPrefab.name = SceneName;

            m_LoadedPrefab.transform.position = Position;
            m_LoadedPrefab.transform.eulerAngles = Rotation;
            m_LoadedPrefab.transform.localScale = LocalScale;
            m_LoadedPrefab.layer = Layer;
            m_LoadedPrefab.tag = Tag;

            for (int i = 0; i < LightMapDatas.Count; i++)
            {
                Transform child = m_LoadedPrefab.transform.Find(LightMapDatas[i].path);
                if (child != null)
                {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    mr.lightmapIndex = LightMapDatas[i].lightmapIndex;
                    mr.lightmapScaleOffset = LightMapDatas[i].lightmapScaleOffset;
                    mr.lightProbeUsage = LightMapDatas[i].lightProbeUsage;
                    mr.realtimeLightmapIndex = LightMapDatas[i].realtimeLightmapIndex;
                    mr.realtimeLightmapScaleOffset = LightMapDatas[i].realtimeLightmapScaleOffset;
                }
            }
        }
        public bool OnShow(Transform parent)
        {
            if (m_LoadedPrefab == null)
            {
#if UNITY_EDITOR
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ResPath);
                m_LoadedPrefab = UnityEngine.Object.Instantiate<GameObject>(obj);

                GameObject parentobj = GameObject.Find(SceneParent);
                m_LoadedPrefab.transform.SetParent(parentobj.transform);
                m_LoadedPrefab.name = SceneName;
             
                m_LoadedPrefab.transform.position = Position;
                m_LoadedPrefab.transform.eulerAngles = Rotation;
                m_LoadedPrefab.transform.localScale = LocalScale;
                m_LoadedPrefab.layer = Layer;
                m_LoadedPrefab.tag = Tag;

                for (int i = 0; i < LightMapDatas.Count; i++)
                {
                    Transform child = m_LoadedPrefab.transform.Find(LightMapDatas[i].path);
                    if (child != null)
                    {
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        mr.lightmapIndex = LightMapDatas[i].lightmapIndex;
                        mr.lightmapScaleOffset = LightMapDatas[i].lightmapScaleOffset;
                        mr.lightProbeUsage = LightMapDatas[i].lightProbeUsage;
                        mr.realtimeLightmapIndex = LightMapDatas[i].realtimeLightmapIndex;
                        mr.realtimeLightmapScaleOffset = LightMapDatas[i].realtimeLightmapScaleOffset;
                    }
                }
                Debug.Log("Node Created " + Time.time);
#else
                if (Application.isPlaying)
                {
                    GameCore.ResMgr.Instance.LoadInstantiateObjectAsync(ResID, true, 0, DelegateResourceCallBack);
                }

#endif
                return true;
            }
            return false;
        }
        public SceneNode()
        {
        }

        public SceneNode(Bounds bounds, Vector3 position, Vector3 rotation, Vector3 size, string resPath, string scenePath)
        {
            Bounds = bounds;
            Position = position;
            Rotation = rotation;
            LocalScale = size;
            ResPath = resPath;
            ScenePath = scenePath;
        }


    }
}