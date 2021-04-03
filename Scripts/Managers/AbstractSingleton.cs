using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{

    public abstract class AbstractSingleton<T> : MonoBehaviour where T : AbstractSingleton<T>
    {
        public static T Instance { get; protected set; }

        protected void Awake()
        {
            if (Instance == null)
            {
                Debug.Log($"Awake {typeof(T)}");
                Instance = this as T;
                Initialize();
                return;
            }

            DestroyImmediate(gameObject);
        }

        protected abstract void Initialize();
    }
}
