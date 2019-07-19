using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SceneSeparate
{
    /// <summary>
    /// 线性四叉树
    /// 节点字典存放叶节点Morton作为Key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinearSceneQuadTree<T> : LinearSceneTree<T> where T : ISceneObject, ISOLinkedListNode
    {
        private float m_DeltaWidth;
        private float m_DeltaHeight;

        public LinearSceneQuadTree(Vector3 center, Vector3 size, int maxDepth) : base(center, size, maxDepth)
        {
            m_DeltaWidth = m_Bounds.size.x / m_Cols;
            m_DeltaHeight = m_Bounds.size.z / m_Cols;
        }
        public override void Add(T item)
        {
            if (item == null)
                return;
            if (m_Bounds.Intersects(item.Bounds))
            {
                if (m_MaxDepth == 0)
                {
                    if (m_Nodes.ContainsKey(0) == false)
                        m_Nodes[0] = new LinearSceneTreeLeaf<T>();
                    var node = m_Nodes[0].Insert(item);
                    item.SetLinkedListNode<T>(0, node);
                }
                else
                {
                    InsertToNode(item, 0, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
                }
            }
        }
        private bool InsertToNode(T obj, int depth, float centerx, float centerz, float sizex, float sizez)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(centerx, centerz);
                if (m_Nodes.ContainsKey(m) == false)
                    m_Nodes[m] = new LinearSceneTreeLeaf<T>();
                var node = m_Nodes[m].Insert(obj);
                obj.SetLinkedListNode<T>(m, node);
                return true;
            }
            else
            {
                int colider = 0;
                float minx = obj.Bounds.min.x;
                float minz = obj.Bounds.min.z;
                float maxx = obj.Bounds.max.x;
                float maxz = obj.Bounds.max.z;

                if (minx <= centerx && minz <= centerz)
                    colider |= 1;
                if (minx <= centerx && maxz >= centerz)
                    colider |= 2;
                if (maxx >= centerx && minz <= centerz)
                    colider |= 4;
                if (maxx >= centerx && maxz >= centerz)
                    colider |= 8;
                float sx = sizex * 0.5f, sz = sizez * 0.5f;

                bool insertresult = false;
                if ((colider & 1) != 0)
                    insertresult = insertresult | InsertToNode(obj, depth + 1, centerx - sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                if ((colider & 2) != 0)
                    insertresult = insertresult | InsertToNode(obj, depth + 1, centerx - sx * 0.5f, centerz + sz * 0.5f, sx, sz);
                if ((colider & 4) != 0)
                    insertresult = insertresult | InsertToNode(obj, depth + 1, centerx + sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                if ((colider & 8) != 0)
                    insertresult = insertresult | InsertToNode(obj, depth + 1, centerx + sx * 0.5f, centerz + sz * 0.5f, sx, sz);
                return insertresult;
            }
        }
        private uint Morton2FromWorldPos(float x, float z)
        {
            uint px = (uint)Mathf.FloorToInt((x - m_Bounds.min.x) / m_DeltaWidth);
            uint pz = (uint)Mathf.FloorToInt((z - m_Bounds.min.z) / m_DeltaHeight);
            return MortonCodeUtil.EncodeMorton2(px, pz);
        }

        public static implicit operator bool(LinearSceneQuadTree<T> tree)
        {
            return tree != null;
        }

        #region 碰撞和遍历
        private void TriggerToNodeByCamera(IDetector detector, TriggerHandle<T> handle, int depth, TreeCullingCode cullingCode, float centerx, float centerz, float sizex,
      float sizez)
        {
            if (cullingCode.IsCulled())
                return;
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(centerx, centerz);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            }
            else
            {
                float sx = sizex * 0.5f, sz = sizez * 0.5f;
                int leftbottommiddle = detector.GetDetectedCode(centerx - sx, m_Bounds.min.y, centerz, true);
                int middlebottommiddle = detector.GetDetectedCode(centerx, m_Bounds.min.y, centerz, true);
                int rightbottommiddle = detector.GetDetectedCode(centerx + sx, m_Bounds.min.y, centerz, true);
                int middlebottomback = detector.GetDetectedCode(centerx, m_Bounds.min.y, centerz - sz, true);
                int middlebottomforward = detector.GetDetectedCode(centerx, m_Bounds.min.y, centerz + sz, true);

                int lefttopmiddle = detector.GetDetectedCode(centerx - sx, m_Bounds.max.y, centerz, true);
                int middletopmiddle = detector.GetDetectedCode(centerx, m_Bounds.max.y, centerz, true);
                int righttopmiddle = detector.GetDetectedCode(centerx + sx, m_Bounds.max.y, centerz, true);
                int middletopback = detector.GetDetectedCode(centerx, m_Bounds.max.y, centerz - sz, true);
                int middletopforward = detector.GetDetectedCode(centerx, m_Bounds.max.y, centerz + sz, true);

                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftbottomback = cullingCode.leftbottomback,
                    leftbottomforward = leftbottommiddle,
                    lefttopback = cullingCode.lefttopback,
                    lefttopforward = lefttopmiddle,
                    rightbottomback = middlebottomback,
                    rightbottomforward = middlebottommiddle,
                    righttopback = middletopback,
                    righttopforward = middletopmiddle,
                }, centerx - sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftbottomback = leftbottommiddle,
                    leftbottomforward = cullingCode.leftbottomforward,
                    lefttopback = lefttopmiddle,
                    lefttopforward = cullingCode.lefttopforward,
                    rightbottomback = middlebottommiddle,
                    rightbottomforward = middlebottomforward,
                    righttopback = middletopmiddle,
                    righttopforward = middletopforward,
                }, centerx - sx * 0.5f, centerz + sz * 0.5f, sx, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftbottomback = middlebottomback,
                    leftbottomforward = middlebottommiddle,
                    lefttopback = middletopback,
                    lefttopforward = middletopmiddle,
                    rightbottomback = cullingCode.rightbottomback,
                    rightbottomforward = rightbottommiddle,
                    righttopback = cullingCode.righttopback,
                    righttopforward = righttopmiddle,
                }, centerx + sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftbottomback = middlebottommiddle,
                    leftbottomforward = middlebottomforward,
                    lefttopback = middletopmiddle,
                    lefttopforward = middletopforward,
                    rightbottomback = rightbottommiddle,
                    rightbottomforward = cullingCode.rightbottomforward,
                    righttopback = righttopmiddle,
                    righttopforward = cullingCode.righttopforward,
                }, centerx + sx * 0.5f, centerz + sz * 0.5f, sx, sz);
            }
        }

        private void TriggerToNode(IDetector detector, TriggerHandle<T> handle, int depth, float centerx, float centerz, float sizex,
            float sizez)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(centerx, centerz);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            }
            else
            {

                int colider = detector.GetDetectedCode(centerx, m_Bounds.center.y, centerz, true);

                float sx = sizex * 0.5f, sz = sizez * 0.5f;

                if ((colider & 1) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerx - sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                if ((colider & 2) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerx - sx * 0.5f, centerz + sz * 0.5f, sx, sz);
                if ((colider & 4) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerx + sx * 0.5f, centerz - sz * 0.5f, sx, sz);
                if ((colider & 8) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerx + sx * 0.5f, centerz + sz * 0.5f, sx, sz);
            }
        }

        public override void Trigger(IDetector detector, TriggerHandle<T> handle)
        {
            if (handle == null)
                return;
            if (detector.UseCameraCulling)
            {
                TreeCullingCode code = new TreeCullingCode()
                {
                    leftbottomback = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.min.y, m_Bounds.min.z, true),
                    leftbottomforward = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.min.y, m_Bounds.max.z, true),
                    lefttopback = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.min.z, true),
                    lefttopforward = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.max.z, true),
                    rightbottomback = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.min.z, true),
                    rightbottomforward = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.max.z, true),
                    righttopback = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.max.y, m_Bounds.min.z, true),
                    righttopforward = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.max.y, m_Bounds.max.z, true),
                };
                TriggerToNodeByCamera(detector, handle, 0, code, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x,
                    m_Bounds.size.z);
            }
            else if (detector.IsRebuild)
                TriggerDetectorNearToFar(detector, handle);
            else
            {
                if (m_MaxDepth == 0)
                {
                    if (m_Nodes.ContainsKey(0) && m_Nodes[0] != null)
                    {
                        m_Nodes[0].Trigger(detector, handle);
                    }
                }
                else
                {
                    TriggerToNode(detector, handle, 0, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
                }

            }
        }
        /// <summary>
        /// 数组顺序对应的二维空间位置关系
        ///  |8|1|2|
        ///  |7|0|3|
        ///  |6|5|4|
        /// </summary>
        private Vector2[] directs = new Vector2[9] {
             new Vector2(0, 0) , new Vector2(0, 1) , new Vector2(1, 1) , new Vector2(1, 0),
            new Vector2(1, -1) ,new Vector2(0, -1) ,new Vector2(-1, -1),
            new Vector2(-1, 0), new Vector2(-1, 1)
        };
        //深度遍历的方向图
        Dictionary<int, int[]> DirectParams = new Dictionary<int, int[]>
        {
            {2,new int[]{ 2,1,3} },
            {4,new int[]{ 4,3,5} },
            {6,new int[]{ 6,5,7} },
            {8,new int[]{ 8,7,1} },
            {1,new int[]{ 1} },
            {3,new int[]{ 3} },
            {5,new int[]{ 5} },
            {7,new int[]{ 7} },
        };

        private void TriggerDetectorNearToFar(IDetector detector, TriggerHandle<T> handle)
        {
            uint row = (uint)Mathf.FloorToInt((detector.Position.x - m_Bounds.min.x) / m_DeltaWidth);
            uint col = (uint)Mathf.FloorToInt((detector.Position.z - m_Bounds.min.z) / m_DeltaHeight);
            uint maxcol = (uint)Mathf.Pow(2, m_MaxDepth);
            float bias = Mathf.Cos(22.5f * Mathf.Deg2Rad);
            int direct = 1;
            if (detector.Rotation != null)
            { 
                direct = Mathf.FloorToInt(((detector.Rotation.y + 360.0f) % 360.0f + 22.5f) / 45.0f)+1;
            }
            //完整遍历的最大N
            int circleN = (int)Mathf.Min(maxcol - row, maxcol - col, row, col);
            TriggerOneCircleNodes(detector, (int)row, (int)col, 0, direct, (int)maxcol, handle);
           /* 
            if (row <= maxcol && col <= maxcol)
            {
                uint m = MortonCodeUtil.EncodeMorton2(row, col);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            }
            for (int i = 1; i < 9; i++)
            {
                uint nrow = row + (uint)directs[i].x;
                uint ncol = col + (uint)directs[i].y;
                TriggerOneNode(detector, nrow, ncol, i, maxcol, handle);
            }*/
        }

      

        //广度遍历
        private void TriggerOneCircleNodes(IDetector detector, int row, int col, int n, int direct, int maxcol, TriggerHandle<T> handle)
        {
            if (n > maxcol)
                return;
            //遍历一周 距离中心距离是n 行列的周期是n 值域是 -n到n
            Vector2 dir = directs[direct];
            Vector2 speed1 = Vector2.zero;
            Vector2 speed2 = Vector2.zero;

            //计算初始速度
            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                speed1.x = (int)dir.x * (-1);
                speed2.y = (int)dir.y * (-1);
            }
            else if (dir.x == 0)
            {
                speed1.x = -1;
                speed2.x = 1;
            }
            else if (dir.y == 0)
            {
                speed1.y = -1;
                speed2.y = 1;
            }
            //起始点
            int beginrow = row + n * (int)dir.x;
            int begincol = col + n * (int)dir.y;

            int endrow = row - n * (int)dir.x;
            int endcol = col - n * (int)dir.y;
            
            bool beginInRegion = beginrow < maxcol && begincol < maxcol && beginrow >= 0 && begincol >= 0;
            //起始点判断
            if (beginInRegion)
            {
                TriggeLeafNode(detector, (uint)beginrow, (uint)begincol, handle);
            }
            int step1x = beginrow;
            int step1y = begincol;
            int step2x = beginrow;
            int step2y = begincol;
            //顺时针和逆时针各走半个周长
            //for (int i = 1; i < 4 * n - 1; i++)
            {
                int count1 = 0;
                int count2 = 0;
                MoveOneStep(detector, row, col, ref step1x, ref step1y, maxcol, ref speed1, direct, n ,ref count1, handle);
                MoveOneStep(detector, row, col, ref step2x, ref step2y, maxcol, ref speed2, direct,n,ref count2, handle);
            }

            //终点判断
            if (endrow < maxcol && endcol < maxcol && endrow >= 0 && endcol >= 0)
            {
                TriggeLeafNode(detector, (uint)endrow, (uint)endcol, handle);
            }

            TriggerOneCircleNodes(detector, row, col, n + 1, direct, maxcol, handle);
        }

        private void MoveOneStep(IDetector detector, int row, int col, ref int stepx, ref int stepy, int maxcol, ref Vector2 speed, int direct, int depth,ref int count, TriggerHandle<T> handle)
        {
            if (count >= 4 * depth-1)
            {
                return ;
            }
            int a = (direct % 2) == 0 ? 2*depth : depth;
            int nextCornerX = stepx + (int)speed.x * a;
            int nextCornerY = stepy + (int)speed.y * a;
            bool nextCorInRegion = nextCornerX < maxcol && nextCornerY < maxcol && nextCornerX >= 0 && nextCornerY >= 0;

            //移动一步
            int nextx = stepx + (int)speed.x;
            int nexty = stepy + (int)speed.y;
            //下一个点在不在区域内
            bool nextIn = (nextx < maxcol && nexty < maxcol && nextx >= 0 && nexty >= 0);
            if (nextIn)
            {
                stepx = nextx;
                stepy = nexty;
                count += 1;
                TriggeLeafNode(detector, (uint)stepx, (uint)stepy, handle);
            }
            else
            {
                //下一个拐点不在区域内
                if (!nextCorInRegion)
                {
                    //跳到拐点修改速度
                    stepx = nextCornerX;
                    stepy = nextCornerY;
                    speed.x -= (int)Mathf.Sign(nextCornerX);
                    speed.y -= (int)Mathf.Sign(nextCornerY);
                    count += 2*depth;
                }
                else//求交点跳到交点
                {
                    int stepx2 = stepx == nextCornerX ? stepx:(stepx* nextCornerX < 0?0:maxcol-1);
                    int stepy2 = stepy == nextCornerY ? stepy : (stepy * nextCornerY < 0 ? 0 : maxcol - 1);
                    count += Mathf.Abs(stepx2 - stepx + stepy2 - stepy);
                }
            }
            MoveOneStep(detector, row, col, ref stepx, ref stepy, maxcol, ref speed, direct, depth, ref count, handle);
            /*
            //沿速度方向走一格
            stepx += (int)speed.x;
            stepy += (int)speed.y;
            //是否在范围内
            if (stepx < maxcol && stepy < maxcol && stepx >= 0 && stepy >= 0)
            {
                TriggeLeafNode(detector, (uint)stepx, (uint)stepy, handle);
            }
            //查看是否在拐点 在拐点修改速度
            int s1 = (stepx - row) / depth;
            int s2 = (stepy - col) / depth;
            if (Mathf.Abs(s1) == 1 && Mathf.Abs(s2) == 1)
            {
                speed.x -= s1;
                speed.y -= s2;
            }*/
        }

        //深度遍历
        private void TriggerOneNode(IDetector detector, uint row, uint col, int direct, uint maxcol, TriggerHandle<T> handle)
        {
            if (row < maxcol && col < maxcol)
            {
                TriggeLeafNode(detector, row, col, handle);
         
                int[] nextDirs = DirectParams[direct];
                for (int i = 0; i < nextDirs.Length; i++)
                {
                    uint nrow = row + (uint)directs[nextDirs[i]].x;
                    uint ncol = col + (uint)directs[nextDirs[i]].y;
                    TriggerOneNode(detector, nrow, ncol, nextDirs[i], maxcol, handle);
                }
            }
        }

        private void TriggeLeafNode(IDetector detector,uint row , uint col, TriggerHandle<T> handle)
        {
            uint m = MortonCodeUtil.EncodeMorton2((uint)row, (uint)col);
            if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
            {
                m_Nodes[m].Trigger(detector, handle);
            }
        }
        #endregion

#if UNITY_EDITOR
        public override void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj)
        {
            DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj,
                0, new Vector2(m_Bounds.center.x, m_Bounds.center.z), new Vector2(m_Bounds.size.x, m_Bounds.size.z));
        }

        private bool DrawNodeGizmos(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj, int depth, Vector2 center, Vector2 size)
        {
            if (depth < drawMinDepth || depth > drawMaxDepth)
                return false;
            float d = ((float)depth) / m_MaxDepth;
            Color color = Color.Lerp(treeMinDepthColor, treeMaxDepthColor, d);
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(center.x, center.y);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    if (m_Nodes[m].DrawNode(objColor, hitObjColor, drawObj))
                    {
                        Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y),
                            new Vector3(size.x, m_Bounds.size.y, size.y));
                        b.DrawBounds(color);
                        //显示坐标
                        UnityEditor.Handles.Label(b.center + Vector3.up * 3,""+m);
                        return true;
                    }
                }
            }
            else
            {
                bool draw = false;
                float sx = size.x * 0.5f, sz = size.y * 0.5f;
                draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
                draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
                draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));
                draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));

                if (draw)
                {
                    Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y),
                        new Vector3(size.x, m_Bounds.size.y, size.y));
                    b.DrawBounds(color);
                }

                return draw;
            }

            return false;
        }
#endif

    }
}