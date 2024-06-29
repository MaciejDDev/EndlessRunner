using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("GroundPool")]

    [SerializeField] GameObject _ground;
    [SerializeField] int _groundAmount = 5;

    [Header("ObstaclePool")]

    [SerializeField] GameObject _obstacle;
    [SerializeField] int _obstacleAmount = 10;

    public static PoolManager Instance { get; internal set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        UIController.OnLevelRetry += StartLevel;

    }

    private void OnDestroy() => UIController.OnLevelRetry -= StartLevel;


    private void Start()
    {
        ObjectPooling.Instance.Preload(_ground, _groundAmount);
        ObjectPooling.Instance.Preload(_obstacle, _obstacleAmount);
        StartLevel();
    }

    public void StartLevel()
    {
        
        GenerateFloor(new Vector2(-2f,-10f));
    }

    public GameObject GenerateFloor(Vector2 spawnPosition)
    {
        GameObject go = ObjectPooling.Instance.GetObject(_ground);
        go.transform.position = spawnPosition;
        var _gr = go.GetComponent<Ground>();
        _gr.SetHeight();
        _gr.SetFallingRandomnes();

        return go;
    }

    public void DespawnFloor(GameObject objectToDespawn)
    {
        ObjectPooling.Instance.RecicleObject(_ground, objectToDespawn);
    }

    public GameObject GenerateObstacle(Vector2 spawnPosition)
    {
        GameObject go = ObjectPooling.Instance.GetObject(_obstacle);
        go.transform.position = spawnPosition;


        return go;
    }

    public void DespawnObstacle(GameObject objectToDespawn)
    {
        ObjectPooling.Instance.RecicleObject(_obstacle,objectToDespawn);
    }

}
