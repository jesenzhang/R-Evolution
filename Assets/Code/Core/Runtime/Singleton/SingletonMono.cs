using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// Singleton base class.
    /// Derive this class to make it Singleton.
    /// </summary>
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_Instance;

        /// <summary>
        /// Returns the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = (T)FindObjectOfType(typeof(T));

                    if (m_Instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).ToString());
                        m_Instance = obj.AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }

    }
}