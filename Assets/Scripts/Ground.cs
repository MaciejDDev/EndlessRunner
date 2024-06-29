using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ground : MonoBehaviour
{
    
    BoxCollider2D _boxCollider;
    
    float _groundHeight;
    float _groundRight;
    Vector3 _screenRight;
    Vector3 _screenLeft;
    static float _fallSpeed = 1.5f;


    bool _didGenerateFloor = false;
    bool _fallingPlatform = false; 
    bool _wasPlayerOnIt = false;
    List<GameObject> _obstaclesOnIt;
    [SerializeField] Camera _camera;


    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
       
        _screenRight =  _camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, 0f, _camera.nearClipPlane)) ;
        _screenLeft = _camera.ScreenToWorldPoint(new Vector3(0f, 0f, _camera.nearClipPlane));
        Player.Instance.PlayerDied += ResetFloor;
        _obstaclesOnIt = new List<GameObject>();
    }

    private void OnDestroy()
    {
        Player.Instance.PlayerDied -= ResetFloor;
    }
    private void Start()
    {
        SetHeight();
    }
 



    private void FixedUpdate()
    {

        Vector2 position = transform.position;
        position.x -= Player.Instance.GetVelocity() * Time.fixedDeltaTime;

        _groundRight = transform.position.x + (_boxCollider.size.x / 2);

        if (!_didGenerateFloor)
        {
            if (_groundRight < _screenRight.x)
            {
                GenerateFloor();
                _didGenerateFloor = true;
            }
        }
        
        if (_groundRight < _screenLeft.x)
        {
            ResetFloor();
        }


        if ( _fallingPlatform && _wasPlayerOnIt)
        {
            SetHeight();
            position.y -= _fallSpeed * Time.fixedDeltaTime;
        }
        transform.position = position;

    }


    public static float GetFallSpeed()
    {
        return _fallSpeed;
    }

    private void ResetFloor() // Se desactiva el objeto y se mete en la cola otra vez
    {
        if (this.GameObject().activeInHierarchy) 
        {
            PoolManager.Instance.DespawnFloor(this.GameObject());
            _didGenerateFloor = false;
            _fallingPlatform = false;
            _wasPlayerOnIt = false;
            _obstaclesOnIt.Clear();
        }
    }

    public void SetFallingRandomnes()
    { 

        if (UnityEngine.Random.Range(0, 4) == 0)
        {
            _fallingPlatform = true;
        }
    }

    public bool GetIfThisFalls()
    {
        return _fallingPlatform;
    }
    private void GenerateFloor()
    {
        Vector2 spawnPosition = GetSpawnPosition();
        GameObject go = PoolManager.Instance.GenerateFloor(spawnPosition);
        Ground generatedGround = go.GetComponent<Ground>();
        //Generar obstáculos en el suelo nuevo
        int obstacleNumber = UnityEngine.Random.Range(0, 3);
        for(int i = 0; i < obstacleNumber;i++)
        {
            //Cuando las plataformas no sean igual de grandes 
            //NO va a funcionar porque cogemos el collider del suelo anterior
            //Arreglar cogiendo el go de PoolManager.Instance.GenerateFloor(spawnPosition);
            float groundLeft = spawnPosition.x - (_boxCollider.size.x / 2) + 1;
            float grounRight = spawnPosition.x + (_boxCollider.size.x / 2) - 1;
            float groundHeight = spawnPosition.y + (_boxCollider.size.y / 2);

            float spawnX = UnityEngine.Random.Range(groundLeft, grounRight);
            Vector2 obstaclePosition = new Vector2(spawnX, groundHeight);
            GameObject obsgo =PoolManager.Instance.GenerateObstacle(obstaclePosition);
           
            generatedGround.AddToObstaclesOnIt(obsgo);
            Obstacle obs = obsgo.GetComponent<Obstacle>();
            if (generatedGround.GetIfThisFalls() )
            {
                obs.SetOnFallingPlatform();
            }


            
        }


    }

    public void AddToObstaclesOnIt(GameObject obs)
    {
        _obstaclesOnIt.Add(obs);
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 maxJumpDistance = Player.Instance.GetMaxJumpHeightAndWidth(this);
        
        float maxX = maxJumpDistance.x;
        maxX = 0.7f * maxX + _groundRight;
        float minX = _groundRight;
        float actualX = UnityEngine.Random.Range(minX, maxX);

        float spawnX = actualX + (_boxCollider.size.x / 2);
        float spawnY = maxJumpDistance.y - (_boxCollider.size.y / 2);
        if(spawnY > -3f) //Límite para que no salga de la pantalla
        {
            spawnY = -3f;
        }

        Vector2 spawn = new Vector2(spawnX, spawnY);
        return spawn;
    }

    public float GetGroundHeight()
    {
        return _groundHeight;
    }

    internal void SetHeight() //Resetea la altura del suelo al reciclarlo
    {
        _groundHeight = transform.position.y + (_boxCollider.size.y / 2);
    }

    internal void PlayerGrounded()
    {
        if(_fallingPlatform && !_wasPlayerOnIt)
        {
            _wasPlayerOnIt = true;
            foreach (var obstacle in _obstaclesOnIt)
            {
                obstacle.GetComponent<Obstacle>().SetFalling();
            }

        }
    }
}
