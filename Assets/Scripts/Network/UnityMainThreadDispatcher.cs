using System.Collections.Generic;
using UnityEngine;

namespace CrazyRisk.Red
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();
        private static UnityMainThreadDispatcher _instance = null;

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("MainThreadDispatcher");
                    _instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }

        public void Enqueue(System.Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
