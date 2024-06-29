using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector2 _screenLeft;

    bool _isOnFallingPlatform = false;
    bool _isFalling = false;

    private void Awake()
    {
        _screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
        Player.Instance.PlayerDied += ResetObstacle;
    }

    private void OnDestroy()
    {
        Player.Instance.PlayerDied -= ResetObstacle;
    }

    public void ResetObstacle()
    {
        _isOnFallingPlatform = false;
        _isFalling = false;
        PoolManager.Instance.DespawnObstacle(this.GameObject());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 position = transform.position;
        position.x -= Player.Instance.GetVelocity() * Time.fixedDeltaTime;


        //Que desaparezca si sale de la pantalla por la izquierda
        if (position.x < (_screenLeft.x - 5 ))
        {
            ResetObstacle();
        }

        if (_isOnFallingPlatform && _isFalling)
        {
            position.y -= Ground.GetFallSpeed() * Time.fixedDeltaTime;
        }


        transform.position = position;

    }



    public void SetOnFallingPlatform()
    {
        _isOnFallingPlatform = true;
    }
    public void SetFalling()
    {
        _isFalling = true;
    }
}
