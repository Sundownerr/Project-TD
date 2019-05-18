using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utility
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;

        public static T Instance
        {
            get => instance;
            set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        protected virtual void Awake()
        {
            Instance = this as T;
        }
    }

    public class SingletonDDOL<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }
    }
}