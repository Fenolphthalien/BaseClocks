using System.Collections;
using UnityEngine;

namespace BaseClocks
{
    /// <summary>
    /// Singleton to used whenever coroutines need to be run whilst the calling object is disable.
    /// For example during loading
    /// </summary>
    public class BaseClocksCoroutineRunner : MonoBehaviour
    {
        public static BaseClocksCoroutineRunner Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject go = new GameObject();
                    m_Instance = go.AddComponent<BaseClocksCoroutineRunner>();
                }
                return m_Instance;
            }
        }

        private static BaseClocksCoroutineRunner m_Instance;
    }
}
