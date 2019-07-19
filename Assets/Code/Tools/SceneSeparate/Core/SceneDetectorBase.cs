using UnityEngine;
using System.Collections;
namespace SceneSeparate
{
    public abstract class SceneDetectorBase : MonoBehaviour, IDetector
    {

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public Vector3 Rotation
        {
            get { return transform.rotation.eulerAngles; }
        }

        public abstract bool UseCameraCulling { get; }

        public abstract bool IsRebuild { get; }

        public abstract bool IsDetected(Bounds bounds);

        public abstract int GetDetectedCode(float x, float y, float z, bool ignoreY);

        //public abstract int DetecedCode2D(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ);
        //public abstract int DetecedCode3D(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ);

        //public abstract int DetectedCode(Bounds bounds, SceneSeparateTreeType treeType);

        //public abstract int DetecedCode(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ, SceneSeparateTreeType treeType);
    }
}