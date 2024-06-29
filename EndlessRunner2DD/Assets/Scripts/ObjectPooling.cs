using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{


    static Dictionary<int, Queue<GameObject>> _pool = new Dictionary<int, Queue<GameObject>>();
    static Dictionary<int, GameObject> _parents = new Dictionary<int, GameObject>();
    public static ObjectPooling Instance { get; internal set; }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }



    public void Preload(GameObject objectToPool, int amount)
    {
        int id = objectToPool.GetInstanceID();

        GameObject parent = new GameObject();
        parent.name = objectToPool.name + "Pool";
        _parents.Add(id, parent);

        _pool.Add(id, new Queue<GameObject>());

        for (int i = 0; i < amount; i++)
        {
            CreateObject(objectToPool);
        }

    }

    public void CreateObject(GameObject objectToPool)
    {
        int id = objectToPool.GetInstanceID();

        GameObject go = Instantiate(objectToPool) as GameObject;
        go.transform.SetParent(GetParent(id).transform);
        go.SetActive(false);

        _pool[id].Enqueue(go);

    }

    public GameObject GetParent(int id)
    {
        GameObject parent;
        _parents.TryGetValue(id, out parent);

        return parent;
    }

    public GameObject GetObject(GameObject objectToPool)
    {
        int id = objectToPool.GetInstanceID();

        if (_pool[id].Count == 0)
            CreateObject(objectToPool);

        GameObject go = _pool[id].Dequeue();
        go.SetActive(true);
        return go;
    }

    public void RecicleObject(GameObject objectToPool, GameObject objectToRecicle)
    {
        int id = objectToPool.GetInstanceID();

        _pool[id].Enqueue(objectToRecicle);
        objectToRecicle.SetActive(false);
    }
}
