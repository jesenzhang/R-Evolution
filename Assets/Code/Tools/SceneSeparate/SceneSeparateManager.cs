using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SceneSeparate
{
    public class SceneSeparateManager : MonoBehaviour
    {
        public SceneSeparateData data;

        public int oneFrameDoCount = 5;

        private WaitForEndOfFrame m_WaitForFrame;
        /// <summary>
        /// 当前场景资源四叉树/八叉树
        /// </summary>
        private ISeparateTree<SceneObject> m_Tree;

        /// <summary>
        /// 刷新时间
        /// </summary>
        private float m_RefreshTime;
        /// <summary>
        /// 销毁时间
        /// </summary>
        private float m_DestroyRefreshTime;

        private Vector3 m_OldRefreshPosition;
        private Vector3 m_OldDestroyRefreshPosition;

        /// <summary>
        /// 异步任务队列
        /// </summary>
        private Queue<SceneObject> m_ProcessTaskQueue;

        /// <summary>
        /// 已加载的物体列表（频繁移除与添加使用双向链表）
        /// </summary>
        private LinkedList<SceneObject> m_LoadedObjectLinkedList;

        /// <summary>
        /// 待销毁物体列表
        /// </summary>
        private PriorityQueue<SceneObject> m_PreDestroyObjectQueue;

        private TriggerHandle<SceneObject> m_TriggerHandle;

        private bool m_IsTaskRunning;

        private bool m_IsInitialized;
        private TreeType m_TreeType = TreeType.LinearQuadTree;
        private int m_MaxCreateCount;
        private int m_MinCreateCount;
        private float m_MaxRefreshTime;
        private float m_MaxDestroyTime;
        private bool m_Asyn;

        private IDetector m_CurrentDetector;

        public SceneDetectorBase detector;

        private bool m_EndInit = false;

        private bool m_NeedUpdate= false;

        public WaitForEndOfFrame WaitForFrame {
            get
            {
                if (m_WaitForFrame == null)
                    m_WaitForFrame = new WaitForEndOfFrame();
                return m_WaitForFrame;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            //Init(data.bounds.center, data.bounds.size, data.asyn, treeType);
            Init(data.bounds.center, data.bounds.size, data.asyn, data.maxCreateCount, data.minCreateCount, data.maxRefreshTime, data.maxDestroyTime, data.treeType,data.treeDepth);
            for (int i = 0; i < data.nodes.Count; i++)
            {
                AddSceneBlockObject(data.nodes[i]);
            }
            Debug.Log("ReBuildScene " + Time.time);
            ReBuildScene();
        }

        void Update()
        {
            if(m_NeedUpdate)
                RefreshDetector(detector);
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="center">场景区域中心</param>
        /// <param name="size">场景区域大小</param>
        /// <param name="asyn">是否异步</param>
        /// <param name="maxCreateCount">最大创建数量</param>
        /// <param name="minCreateCount">最小创建数量</param>
        /// <param name="maxRefreshTime">更新区域时间间隔</param>
        /// <param name="maxDestroyTime">检查销毁时间间隔</param>
        /// <param name="quadTreeDepth">四叉树深度</param>
        public void Init(Vector3 center, Vector3 size, bool asyn, int maxCreateCount, int minCreateCount, float maxRefreshTime, float maxDestroyTime, TreeType treeType, int quadTreeDepth = 5)
        {
            if (m_IsInitialized)
                return;
            CreateTree(center, size, treeType, quadTreeDepth);
            m_LoadedObjectLinkedList = new LinkedList<SceneObject>();
            m_PreDestroyObjectQueue = new PriorityQueue<SceneObject>(new SceneObjectWeightComparer());
            m_TriggerHandle = new TriggerHandle<SceneObject>(this.TriggerHandle);
            m_MaxCreateCount = Mathf.Max(0, maxCreateCount);
            m_MinCreateCount = Mathf.Clamp(minCreateCount, 0, m_MaxCreateCount);
            m_MaxRefreshTime = maxRefreshTime;
            m_MaxDestroyTime = maxDestroyTime;
            m_Asyn = asyn;

            m_IsInitialized = true;

            m_RefreshTime = maxRefreshTime;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="center">场景区域中心</param>
        /// <param name="size">场景区域大小</param>
        /// <param name="asyn">是否异步</param>
        public void Init(Vector3 center, Vector3 size, bool asyn, TreeType treeType)
        {
            Init(center, size, asyn, 25, 15, 1, 5, treeType);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="center">场景区域中心</param>
        /// <param name="size">场景区域大小</param>
        /// <param name="asyn">是否异步</param>
        /// <param name="maxCreateCount">更新区域时间间隔</param>
        /// <param name="minCreateCount">检查销毁时间间隔</param>
        public void Init(Vector3 center, Vector3 size, bool asyn, int maxCreateCount, int minCreateCount, TreeType treeType)
        {
            Init(center, size, asyn, maxCreateCount, minCreateCount, 1, 5, treeType);
        }


        public void CreateTree(Vector3 center, Vector3 size, TreeType treeType, int quadTreeDepth = 5)
        {
            m_TreeType = treeType;
            switch (m_TreeType)
            {
                case TreeType.LinearOcTree:
                    m_Tree = new LinearSceneOcTree<SceneObject>(center, size, quadTreeDepth);
                    break;
                case TreeType.LinearQuadTree:
                    m_Tree = new LinearSceneQuadTree<SceneObject>(center, size, quadTreeDepth);
                    break;
                case TreeType.OcTree:
                    m_Tree = new SceneTree<SceneObject>(center, size, quadTreeDepth, true);
                    break;
                case TreeType.QuadTree:
                    m_Tree = new SceneTree<SceneObject>(center, size, quadTreeDepth, false);
                    break;
                default:
                    m_Tree = new LinearSceneQuadTree<SceneObject>(center, size, quadTreeDepth);
                    break;
            }
        }

        void OnDestroy()
        {
            if (m_Tree != null)
                m_Tree.Clear();
            m_Tree = null;
            if (m_ProcessTaskQueue != null)
                m_ProcessTaskQueue.Clear();
            if (m_LoadedObjectLinkedList != null)
                m_LoadedObjectLinkedList.Clear();
            m_ProcessTaskQueue = null;
            m_LoadedObjectLinkedList = null;
            m_TriggerHandle = null;
        }

        /// <summary>
        /// 添加场景物体
        /// </summary>
        /// <param name="obj"></param>
        public void AddSceneBlockObject(ISceneObject obj)
        {
            if (!m_IsInitialized)
                return;
            if (m_Tree == null)
                return;
            if (obj == null)
                return;
            //使用SceneObject包装
            SceneObject sbobj = new SceneObject(obj);
            m_Tree.Add(sbobj);
            //如果当前触发器存在，直接物体是否可触发，如果可触发，则创建物体
            if (m_CurrentDetector != null && m_CurrentDetector.IsDetected(sbobj.Bounds))
            {
                DoCreateInternal(sbobj);
            }
        }

        //重建场景
        public void ReBuildScene()
        {
            if (!m_IsInitialized)
                return;
            if (detector == null)
            {
                GameObject obj = new GameObject("SceneNearFarDetector");
                detector = obj.AddComponent<SceneNearFarDetector>();
                obj.transform.position = Vector3.zero;
                GameObject player = GameObject.Find("ENTITY_OBJECT/PLAYER_MAIN");
                if (player != null && player.transform.childCount>=1)
                {
                    obj.transform.position = player.transform.GetChild(0).position;
                }
            }
            
            m_OldRefreshPosition = detector.Position; 
            m_CurrentDetector = detector;
            //进行触发检测
            m_Tree.Trigger(detector, m_TriggerHandle);
        }

        /// <summary>
        /// 刷新触发器
        /// </summary>
        /// <param name="detector">触发器</param>
        public void RefreshDetector(IDetector detector)
        {
            if (!m_IsInitialized)
                return;
            //只有坐标发生改变才调用
            if (!m_EndInit || m_OldRefreshPosition != detector.Position)
            {
                m_EndInit = true;
                m_RefreshTime += Time.deltaTime;
                //达到刷新时间才刷新，避免区域更新频繁
                if (m_RefreshTime > m_MaxRefreshTime)
                {
                    m_OldRefreshPosition = detector.Position;
                    m_RefreshTime = 0;
                    m_CurrentDetector = detector;
                    //进行触发检测
                    m_Tree.Trigger(detector, m_TriggerHandle);
                    //标记超出区域的物体
                    MarkOutofBoundsObjs();
                    //m_IsInitLoadComplete = true;
                }
            }
            if (m_OldDestroyRefreshPosition != detector.Position)
            {
                if (m_PreDestroyObjectQueue != null && m_PreDestroyObjectQueue.Count >= m_MaxCreateCount && m_PreDestroyObjectQueue.Count > m_MinCreateCount)
                //if (m_PreDestroyObjectList != null && m_PreDestroyObjectList.Count >= m_MaxCreateCount)
                {
                    m_DestroyRefreshTime += Time.deltaTime;
                    if (m_DestroyRefreshTime > m_MaxDestroyTime)
                    {
                        m_OldDestroyRefreshPosition = detector.Position;
                        m_DestroyRefreshTime = 0;
                        //删除超出区域的物体
                        DestroyOutOfBoundsObjs();
                    }
                }
            }
        }

        /// <summary>
        /// 四叉树触发处理函数
        /// </summary>
        /// <param name="data">与当前包围盒发生触发的场景物体</param>
        void TriggerHandle(SceneObject data)
        {
            if (data == null)
                return;
            if (data.Flag == SceneObject.CreateFlag.Old) //如果发生触发的物体已经被创建则标记为新物体，以确保不会被删掉
            {
                data.Weight++;
                data.Flag = SceneObject.CreateFlag.New;
            }
            else if (data.Flag == SceneObject.CreateFlag.OutofBounds)//如果发生触发的物体已经被标记为超出区域，则从待删除列表移除该物体，并标记为新物体
            {
                data.Flag = SceneObject.CreateFlag.New;
                //if (m_PreDestroyObjectList.Remove(data))
                {
                    m_LoadedObjectLinkedList.AddFirst(data);

                }
            }
            else if (data.Flag == SceneObject.CreateFlag.None) //如果发生触发的物体未创建则创建该物体并加入已加载的物体列表
            {
                DoCreateInternal(data);
            }
        }

        //执行创建物体
        private void DoCreateInternal(SceneObject data)
        {
            //加入已加载列表
            m_LoadedObjectLinkedList.AddFirst(data);
            //创建物体
            CreateObject(data, m_Asyn);
        }

        /// <summary>
        /// 标记离开视野的物体
        /// </summary>
        void MarkOutofBoundsObjs()
        {
            if (m_LoadedObjectLinkedList == null)
                return;

            var node = m_LoadedObjectLinkedList.First;
            while (node != null)
            {
                var obj = node.Value;
                if (obj.Flag == SceneObject.CreateFlag.Old)//已加载物体标记仍然为Old，说明该物体没有进入触发区域，即该物体在区域外
                {
                    obj.Flag = SceneObject.CreateFlag.OutofBounds;
                    if (m_MinCreateCount == 0)//如果最小创建数为0直接删除
                    {
                        DestroyObject(obj, m_Asyn);
                    }
                    else
                    {
                        m_PreDestroyObjectQueue.Push(obj);//加入待删除队列
                    }

                    var next = node.Next;
                    m_LoadedObjectLinkedList.Remove(node);
                    node = next;
                }
                else
                {
                    obj.Flag = SceneObject.CreateFlag.Old;
                    node = node.Next;
                }
            }
        }

        /// <summary>
        /// 删除超出区域外的物体
        /// </summary>
        void DestroyOutOfBoundsObjs()
        {
            while (m_PreDestroyObjectQueue.Count > m_MinCreateCount)
            {

                var obj = m_PreDestroyObjectQueue.Pop();
                if (obj == null)
                    continue;
                if (obj.Flag == SceneObject.CreateFlag.OutofBounds)
                {
                    DestroyObject(obj, m_Asyn);
                }
            }
        }

        /// <summary>
        /// 创建物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="asyn"></param>
        private void CreateObject(SceneObject obj, bool asyn)
        {
            if (obj == null)
                return;
            if (obj.TargetObj == null)
                return;
            if (obj.Flag == SceneObject.CreateFlag.None)
            {
                if (!asyn)
                    CreateObjectSync(obj);
                else
                    ProcessObjectAsyn(obj, true);
                obj.Flag = SceneObject.CreateFlag.New;//被创建的物体标记为New
            }
        }

        /// <summary>
        /// 删除物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="asyn"></param>
        private void DestroyObject(SceneObject obj, bool asyn)
        {
            if (obj == null)
                return;
            if (obj.Flag == SceneObject.CreateFlag.None)
                return;
            if (obj.TargetObj == null)
                return;
            if (!asyn)
                DestroyObjectSync(obj);
            else
                ProcessObjectAsyn(obj, false);
            obj.Flag = SceneObject.CreateFlag.None;//被删除的物体标记为None
        }

        /// <summary>
        /// 同步方式创建物体
        /// </summary>
        /// <param name="obj"></param>
        private void CreateObjectSync(SceneObject obj)
        {
            if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareDestroy)//如果标记为IsPrepareDestroy表示物体已经创建并正在等待删除，则直接设为None并返回
            {
                obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                return;
            }
            obj.OnShow(transform);//执行OnShow
        }

        /// <summary>
        /// 同步方式销毁物体
        /// </summary>
        /// <param name="obj"></param>
        private void DestroyObjectSync(SceneObject obj)
        {
            if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareCreate)//如果物体标记为IsPrepareCreate表示物体未创建并正在等待创建，则直接设为None并放回
            {
                obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                return;
            }
            obj.OnHide();//执行OnHide
        }

        /// <summary>
        /// 异步处理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="create"></param>
        private void ProcessObjectAsyn(SceneObject obj, bool create)
        {
            if (create)
            {
                if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareDestroy)//表示物体已经创建并等待销毁，则设置为None并跳过
                {
                    obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                    return;
                }
                if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareCreate)//已经开始等待创建，则跳过
                    return;
                obj.ProcessFlag = SceneObject.CreatingProcessFlag.IsPrepareCreate;//设置为等待开始创建
            }
            else
            {
                if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareCreate)//表示物体未创建并等待创建，则设置为None并跳过
                {
                    obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                    return;
                }
                if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareDestroy)//已经开始等待销毁，则跳过
                    return;
                obj.ProcessFlag = SceneObject.CreatingProcessFlag.IsPrepareDestroy;//设置为等待开始销毁
            }
            if (m_ProcessTaskQueue == null)
                m_ProcessTaskQueue = new Queue<SceneObject>();
            m_ProcessTaskQueue.Enqueue(obj);//加入
            if (!m_IsTaskRunning)
            {
               // Debug.Log(string.Format("当前帧数 {0} StartCoroutine 任务：{1}", Time.frameCount, m_ProcessTaskQueue.Count));
                StartCoroutine(AsynTaskProcess());//开始协程执行异步任务
            }

        }

        /// <summary>
        /// 异步任务
        /// </summary>
        /// <returns></returns>
        private IEnumerator AsynTaskProcess()
        {
            if (m_ProcessTaskQueue == null)
                yield return 0;
            m_IsTaskRunning = true;

            while (m_ProcessTaskQueue.Count > 0)
            {
                int oneLoop = m_ProcessTaskQueue.Count >= oneFrameDoCount ? oneFrameDoCount : m_ProcessTaskQueue.Count;
                for (int i = 0; i < oneLoop; i++)
                {
                    var obj = m_ProcessTaskQueue.Dequeue();
                    if (obj != null)
                    {
                        if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareCreate)//等待创建
                        {
                            obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                            obj.OnShow(transform);
                        }
                        else if (obj.ProcessFlag == SceneObject.CreatingProcessFlag.IsPrepareDestroy)//等待销毁
                        {
                            obj.ProcessFlag = SceneObject.CreatingProcessFlag.None;
                            obj.OnHide();
                        }
                       // Debug.Log(string.Format("当前帧数 {0}  任务：{1}", Time.frameCount, m_ProcessTaskQueue.Count));
                    }
                }
                yield return WaitForFrame;
            }

            m_IsTaskRunning = false;
        }

        private class SceneObjectWeightComparer : IComparer<SceneObject>
        {

            public int Compare(SceneObject x, SceneObject y)
            {
                if (y.Weight < x.Weight)
                    return 1;
                else if (y.Weight == x.Weight)
                    return 0;
                return -1;
            }
        }

#if UNITY_EDITOR
        
        public void EditorInit()
        {
            CreateTree(data.bounds.center, data.bounds.size, data.treeType, data.treeDepth);
            m_LoadedObjectLinkedList = new LinkedList<SceneObject>();
            m_PreDestroyObjectQueue = new PriorityQueue<SceneObject>(new SceneObjectWeightComparer());
            m_TriggerHandle = new TriggerHandle<SceneObject>(this.TriggerHandle);
            m_MaxCreateCount = Mathf.Max(0, data.maxCreateCount);
            m_MinCreateCount = Mathf.Clamp(data.minCreateCount, 0, data.maxCreateCount);
            m_MaxRefreshTime = data.maxRefreshTime;
            m_MaxDestroyTime = data.maxDestroyTime;
            m_Asyn = data.asyn;
            m_RefreshTime = data.maxRefreshTime;
        }
        public void EditorAddDataObjects()
        {
            for (int i = 0; i < data.nodes.Count; i++)
            {
                EditorAddSceneBlockObject(data.nodes[i]);
            }
        }

        private void EditorAddSceneBlockObject(ISceneObject obj)
        {
            if (m_Tree == null)
                return;
            if (obj == null)
                return;
            //使用SceneObject包装
            SceneObject sbobj = new SceneObject(obj);
            m_Tree.Add(sbobj);
        }

        public void EditorReBuild()
        {
            //进行触发检测
            m_Tree.Trigger(detector, m_TriggerHandle);
        }

        [HideInInspector]
        public int debug_DrawMinDepth = 0;
        [HideInInspector]
        public int debug_DrawMaxDepth = 5;
        [HideInInspector]
        public bool debug_DrawObj = true;

        void OnDrawGizmosSelected()
        {
            Color mindcolor = new Color32(255, 0, 0, 255);
            Color maxdcolor = new Color32(255, 0, 255, 255);
            Color objcolor = Color.cyan;
            Color hitcolor = new Color32(255, 216, 0, 255);
             
            if (data!=null)
                data.bounds.DrawBounds(Color.red);
            if (m_Tree != null)
                m_Tree.DrawTree(mindcolor, maxdcolor, objcolor, hitcolor, debug_DrawMinDepth, debug_DrawMaxDepth, debug_DrawObj);
        }
#endif
    }
}
