using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utility
{
    public class ObjectPool
    {
        public GameObject PoolObject { get; set; }
        public Transform Parent { get; set; }
        public uint PoolLenght { get; set; } = 1;

        public List<GameObject> Objects { get; private set; } = new List<GameObject>();

        public ObjectPool(GameObject poolObject, Transform parent, uint poolLength)
        {
            PoolObject = poolObject;
            Parent = parent;
            PoolLenght = poolLength;
            Initialize();

            void Initialize()
            {
                if (PoolObject == null)
                {
                    Debug.LogError("ObjectPooler missing prefab");
                    return;
                }

                for (int i = 0; i < PoolLenght; i++)
                    CreateObject(Parent);
            }
        }

        public GameObject PopObject()
        {
            for (int i = 0; i < Objects.Count; i++)
                if (!Objects[i].activeSelf)
                {
                    Objects[i].SetActive(true);
                    return Objects[i];
                }

            if (PoolObject == null)
                return null;

            CreateObject(Parent);

            return Objects[Objects.Count - 1];
        }



        void CreateObject(Transform parent)
        {
            Objects.Add(Object.Instantiate(PoolObject, parent));
            Objects[Objects.Count - 1].SetActive(false);
        }

        public void DestroyPool()
        {
            for (int i = Objects.Count - 1; i > 0; i--)
                Object.Destroy(Objects[i]);

            Objects.Clear();
        }

        public void DeactivateAll()
        {
            for (int i = 0; i < Objects.Count; i++)
                Objects[i].SetActive(false);
        }
    }
}