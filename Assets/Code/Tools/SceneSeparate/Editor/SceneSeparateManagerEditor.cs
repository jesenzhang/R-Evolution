using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace SceneSeparate
{
    [CustomEditor(typeof(SceneSeparateManager))]
    public class SceneSeparateManagerEditor : Editor
    {
        private SceneSeparateManager m_Target;
        private string GetScenePath()
        {
           string path = EditorSceneManager.GetActiveScene().path;
           return path.Replace(".unity","");
        }
        SceneSeparateData GetData()
        {
            string dataPath = string.Format("{0}/sceneseparate.asset", GetScenePath());
            string scenePath = GetScenePath();
            if (m_Target.data == null)
            {
                m_Target.data = AssetDatabase.LoadAssetAtPath<SceneSeparateData>(dataPath);
            }
            if (m_Target.data == null)
            {
                SceneSeparateData obj = ScriptableObject.CreateInstance<SceneSeparateData>();
                if (!Directory.Exists(scenePath))
                {
                    Directory.CreateDirectory(scenePath);
                }
                AssetDatabase.CreateAsset(obj, dataPath);
                m_Target.data = obj;
            }
            return m_Target.data;
        }

        void OnEnable()
        {
            m_Target = target as SceneSeparateManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SceneSeparateData separateData = GetData();
            separateData.sceneName = EditorSceneManager.GetActiveScene().name;
            separateData.treeType = (TreeType)EditorGUILayout.EnumPopup("Tree Type树类型:",separateData.treeType);
            separateData.treeDepth = EditorGUILayout.IntField("treeDepth树深度:", separateData.treeDepth);
            separateData.asyn = EditorGUILayout.Toggle("asyn是否异步:", separateData.asyn);
            separateData.maxCreateCount = EditorGUILayout.IntField("maxCreateCount最大创建数量:", separateData.maxCreateCount);
            separateData.minCreateCount =  EditorGUILayout.IntField("minCreateCount最小创建数量:", separateData.minCreateCount);
            separateData.maxRefreshTime = EditorGUILayout.IntField("maxRefreshTime更新区域时间间隔:", separateData.maxRefreshTime);
            separateData.maxDestroyTime = EditorGUILayout.IntField("maxDestroyTime检查销毁时间间隔:", separateData.maxDestroyTime);
            EditorGUILayout.Space();
             
            SerializedObject serializedObject = new UnityEditor.SerializedObject(target);

            SerializedProperty debug_DrawObj = serializedObject.FindProperty("debug_DrawObj");
            debug_DrawObj.boolValue = EditorGUILayout.Toggle("显示树节点物体网格:", debug_DrawObj.boolValue);
            SerializedProperty debug_DrawMinDepth = serializedObject.FindProperty("debug_DrawMinDepth");
            debug_DrawMinDepth.intValue = EditorGUILayout.IntField("树最小深度:", debug_DrawMinDepth.intValue);
            SerializedProperty debug_DrawMaxDepth = serializedObject.FindProperty("debug_DrawMaxDepth");
            debug_DrawMaxDepth.intValue = EditorGUILayout.IntField("树最大深度:", debug_DrawMaxDepth.intValue);
            
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("拾取节点下的物体"))
            {
                Undo.RecordObject(target, "PickChilds");
                PickChilds();
            }

            if (GUILayout.Button("预览四叉树"))
            {
                m_Target.EditorInit();
                m_Target.EditorAddDataObjects();
            }

            if (GUILayout.Button("恢复场景"))
            {
                m_Target.EditorReBuild();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("保存"))
            {
                EditorUtility.SetDirty(GetData());
                Undo.RecordObject(target, "SaveData");
                SaveData();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("移除节点下保存的预制体（记得备份）"))
            {
                Undo.RecordObject(target, "RemoveSavcedChilds");
                RemoveSavcedChilds();
            }
        }

        private void SaveData()
        {
            AssetDatabase.SaveAssets();
        }

        private void PickChilds()
        {
            if (m_Target.transform.childCount == 0)
                return;
            List<SceneNode> list = new List<SceneNode>();
            PickChild(m_Target.transform, list);

            float maxX, maxY, maxZ, minX, minY, minZ;
            maxX = maxY = maxZ = -Mathf.Infinity;
            minX = minY = minZ = Mathf.Infinity;
            if (list.Count > 0)
                GetData().nodes = list;

            for (int i = 0; i < list.Count; i++)
            {
                maxX = Mathf.Max(list[i].Bounds.max.x, maxX);
                maxY = Mathf.Max(list[i].Bounds.max.y, maxY);
                maxZ = Mathf.Max(list[i].Bounds.max.z, maxZ);

                minX = Mathf.Min(list[i].Bounds.min.x, minX);
                minY = Mathf.Min(list[i].Bounds.min.y, minY);
                minZ = Mathf.Min(list[i].Bounds.min.z, minZ);
            }
            Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 center = new Vector3(minX + size.x / 2, minY + size.y / 2, minZ + size.z / 2);
            GetData().bounds = new Bounds(center, size);
        }

        private void RemoveSavcedChilds()
        {
            List<SceneNode> list = GetData().nodes;
            foreach (SceneNode node in list)
            {
                GameObject obj = GameObject.Find(node.ScenePath);
                if (obj != null)
                    GameObject.DestroyImmediate(obj);
            }
        }

        private void PickChild(Transform transform, List<SceneNode> sceneObjectList)
        {
            if (!transform.gameObject.activeSelf)
            {
                return;
            }
            var obj = PrefabUtility.GetCorrespondingObjectFromSource<Object>(transform);

            if (PrefabUtility.GetPrefabAssetType(transform) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(transform) == PrefabAssetType.MissingAsset)
            {
                obj = null;
            }

            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                string ext = Path.GetExtension(path);
                if (ext == ".prefab")
                {
                    var o = GetChildInfo(transform, path);
                    if (o != null)
                        sceneObjectList.Add(o);
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    PickChild(transform.GetChild(i), sceneObjectList);
                }
            }
        }
        //得到节点的路径
        public void GetNodePath(Transform trans, ref string path)
        {
            if (path == "")
            {
                path = trans.name;
            }
            else
            {
                path = trans.name + "/" + path;
            }

            if (trans.parent != null)
            {
                GetNodePath(trans.parent, ref path);
            }
        }

        private SceneNode GetChildInfo(Transform transform, string resPath)
        {
            if (string.IsNullOrEmpty(resPath))
                return null;
            Renderer[] renderers = transform.gameObject.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null || renderers.Length == 0)
                return null;
            Vector3 min = renderers[0].bounds.min;
            Vector3 max = renderers[0].bounds.max;
            string scenePath = "";
            GetNodePath(transform, ref scenePath);
            string parentPath = "";
            string sceneName = "";
            string[] list = scenePath.Split('/');
            if (list.Length > 1)
            {
                int start = list[0].Length + 1;
                int length = scenePath.Length - list[list.Length - 1].Length - list[0].Length - 1;

                parentPath = scenePath.Substring(0, start - 1);
                sceneName = scenePath.Substring(start + length);
            }
            else
            {
                parentPath = "";
                sceneName = scenePath;
            }


            List<LightMapData> m_LightMapDatas = new List<LightMapData>();
            for (int i = 0; i < renderers.Length; i++)
            {
                min = Vector3.Min(renderers[i].bounds.min, min);
                max = Vector3.Max(renderers[i].bounds.max, max);
                LightMapData lightMapData = new LightMapData();
                lightMapData.lightmapIndex = renderers[i].lightmapIndex;
                lightMapData.lightmapScaleOffset = renderers[i].lightmapScaleOffset;
                lightMapData.realtimeLightmapIndex = renderers[i].realtimeLightmapIndex;
                lightMapData.lightProbeUsage = renderers[i].lightProbeUsage;
                string tpath = "";
                GetNodePath(renderers[i].transform, ref tpath);
                tpath = tpath.Replace(scenePath, "");
                if (tpath.StartsWith("/"))
                {
                    tpath = tpath.Substring(1);
                }
                lightMapData.path = tpath;
                m_LightMapDatas.Add(lightMapData);
            }
            Vector3 size = max - min;
            Bounds bounds = new Bounds(min + size / 2, size);
            if (size.x <= 0)
                size.x = 0.2f;
            if (size.y <= 0)
                size.y = 0.2f;
            if (size.z <= 0)
                size.z = 0.2f;
            bounds.size = size;

            SceneNode obj = new SceneNode(bounds, transform.position, transform.eulerAngles, transform.localScale, resPath, scenePath);
            obj.LightMapDatas = m_LightMapDatas;
            obj.SceneName = sceneName;
            obj.SceneParent = parentPath;
            obj.Layer = transform.gameObject.layer;
            obj.Tag = transform.gameObject.tag;
            obj.ResID = 0;// resPath;
            return obj;

        }
    }
}