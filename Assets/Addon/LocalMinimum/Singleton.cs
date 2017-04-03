using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum
{
    public abstract class Singleton<T> : MonoBehaviour where T : Object
    {

        static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        _instance = CreateInstance();
                    }
                }

                return _instance;
            }

        }

        static void SetInstance(T instance)
        {
            _instance = instance;
        }

        protected static T CreateInstance()
        {
            GameObject go = new GameObject("Manager", typeof(T));
            return go.GetComponent<T>();
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
            }
        }
    }

}
