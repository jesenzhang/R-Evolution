using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneSeparate
{
    public class SceneNearFarDetector : SceneDetectorBase
    {
        public override bool UseCameraCulling => false;

        public override bool IsRebuild => true;

        public Vector3 detectorSize;

        protected Bounds m_Bounds;

        protected virtual void RefreshBounds()
        {
            m_Bounds.center = Position;
            m_Bounds.size = detectorSize;
        }

        public override bool IsDetected(Bounds bounds)
        {
            return true;
         //      RefreshBounds();
         //   return bounds.Intersects(m_Bounds);
        }

        public override int GetDetectedCode(float x, float y, float z, bool ignoreY)
        {
            RefreshBounds();
            int code = 0;
            if (ignoreY)
            {
                float minx = m_Bounds.min.x;
                float minz = m_Bounds.min.z;
                float maxx = m_Bounds.max.x;
                float maxz = m_Bounds.max.z;
                if (minx <= x && minz <= z)
                    code |= 1;
                if (minx <= x && maxz >= z)
                    code |= 2;
                if (maxx >= x && minz <= z)
                    code |= 4;
                if (maxx >= x && maxz >= z)
                    code |= 8;
            }
            else
            {
                float minx = m_Bounds.min.x;
                float miny = m_Bounds.min.y;
                float minz = m_Bounds.min.z;
                float maxx = m_Bounds.max.x;
                float maxy = m_Bounds.max.y;
                float maxz = m_Bounds.max.z;
                if (minx <= x && miny <= y && minz <= z)
                    code |= 1;
                if (minx <= x && miny <= y && maxz >= z)
                    code |= 2;
                if (minx <= x && maxy >= y && minz <= z)
                    code |= 4;
                if (minx <= x && maxy >= y && maxz >= z)
                    code |= 8;
                if (maxx >= x && miny <= y && minz <= z)
                    code |= 16;
                if (maxx >= x && miny <= y && maxz >= z)
                    code |= 32;
                if (maxx >= x && maxy >= y && minz <= z)
                    code |= 64;
                if (maxx >= x && maxy >= y && maxz >= z)
                    code |= 128;
            }
            return code;
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Bounds b = new Bounds(transform.position, detectorSize);
            b.DrawBounds(Color.yellow);
        }
#endif
    }
}