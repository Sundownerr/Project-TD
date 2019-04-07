using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public GameObject PoolObject { get; set; }
    public Transform Parent { get; set; }
    public uint PoolLenght { get; set; } = 1;

    List<GameObject> poolList = new List<GameObject>();

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
        for (int i = 0; i < poolList.Count; i++)
            if (!poolList[i].activeSelf)
            {
                poolList[i].SetActive(true);
                return poolList[i];
            }

        if (PoolObject == null)
            return null;

        CreateObject(Parent);

        return poolList[poolList.Count - 1];
    }

   

    void CreateObject(Transform parent)
    {
        poolList.Add(Object.Instantiate(PoolObject, parent));
        poolList[poolList.Count - 1].SetActive(false);
    }

    public void DestroyPool()
    {
        for (int i = poolList.Count - 1; i > 0; i--)
            Object.Destroy(poolList[i]);

        poolList.Clear();
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < poolList.Count; i++)
            poolList[i].SetActive(false);
    }
}