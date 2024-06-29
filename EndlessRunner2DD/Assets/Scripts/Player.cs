using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] Vector2 _velocity;
    
    [Header("JumpParameters")]
    [SerializeField] float _gravity;
    [SerializeField] float _jumpVelocity = 20;
    [SerializeField] float _groundHeight = -5;
    [SerializeField] float _maxPressJumpTime = 0f;
    [SerializeField] float _maxMaxPressJumpTime = 0.35f;
    [SerializeField] float _pressJumpTimer = 0.0f;
    [SerializeField] float _minHeightToJump = 1;

    [SerializeField] bool _isGrounded;
    [SerializeField] bool _isPressingJump;
    
    [Header("XMovement")]
    [SerializeField] float _accelerationX;
    [SerializeField] float _maxAccelerationX  = 10;
    [SerializeField] float _maxVelocityX = 100;

    [Header("Distance")]
    [SerializeField] float _distance = 0f;

    [Header("Raycasting")]
    [SerializeField] float _rayOriginOffsetX = 0.7f;
    
    [SerializeField] GameObject _camera;


    bool _isDead;
    public Action PlayerDied;
    private Vector2 _startingPosition;

    int _groundLayerMask = 6;
    int _obstacleLayerMask = 7;
    SpriteRenderer _playerSprite;
    CameraShake _cameraShake;
    float _dyingHeight = -15f;

    public static Player Instance { get; internal set; }

    private void Awake()
    {
        Instance = this;
        _groundLayerMask = LayerMask.GetMask("Ground");
        _obstacleLayerMask = LayerMask.GetMask("Obstacle");
        _playerSprite = this.GetComponentInChildren<SpriteRenderer>();
        _cameraShake = _camera.GetComponent<CameraShake>();
        UIController.OnLevelRetry += ResetPlayer;
    }

    private void OnDestroy() => UIController.OnLevelRetry -= ResetPlayer;
    void Start()
    {
        _groundHeight = transform.position.y;
        _startingPosition = transform.position;
    }


    void Update()
    {
        CheckInput();

    }

    private void FixedUpdate()
    {
        if (_isDead || !_playerSprite.enabled)
            return;

        Vector2 position = transform.position;

        if (!_isGrounded)
        {
            
            position = UpdateJumpTimerAndYVelocity(position);

            position = CheckStartGrounding(position);

            //Check GAME OVER
            /*if (!_isGrounded)
            {
            }*/
                
                if (position.y < _dyingHeight)
                {
                    Die();
                    return;
                }
        }
        if (CheckWallCollision(position))
            return;

        _distance += _velocity.x * Time.fixedDeltaTime;

        if (_isGrounded)
        {
            UpdateXVelocity(position);

        }

        //Check obstacle collision
        CheckObstacleCollisionX(position);
        CheckObstacleCollisionY(position);

        transform.position = position;
    }

    private bool CheckWallCollision(Vector2 position)
    {
        bool dead = false;
        Vector2 wallRayOrigin = new Vector2(position.x, position.y + 0.3f);
        RaycastHit2D wallHit = Physics2D.Raycast(wallRayOrigin, Vector2.right, _velocity.x * Time.fixedDeltaTime, _groundLayerMask);
        if (wallHit.collider != null)
        {
            Ground ground = wallHit.collider.GetComponent<Ground>();
            if (ground != null)
            {
                if (transform.position.y < ground.GetGroundHeight())
                {
                    Debug.Log("Wall Collision");
                    
                    dead = true;
                    Die();

                }
            }
        }
        Debug.DrawRay(wallRayOrigin, Vector2.right * _velocity.x * Time.fixedDeltaTime, Color.green);
        return dead;
    }

    void Die()
    {
        Debug.Log("Dead");
        _velocity.x = 0;
        _isDead = true;
        //SHOW GAME OVER SCREEN AND SCORE
        _playerSprite.enabled = false;
        _cameraShake.StopShaking();
        PlayerDied?.Invoke();
    }

    private void CheckObstacleCollisionY(Vector2 position)
    {
        Vector2 obstacleRayOrigin = new Vector2(position.x, position.y);
        RaycastHit2D obstHitY = Physics2D.Raycast(obstacleRayOrigin, Vector2.up, _velocity.y * Time.fixedDeltaTime, _obstacleLayerMask);
        if (obstHitY.collider != null)
        {
            Obstacle obstacle = obstHitY.collider.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                Debug.Log("Obstacle Collision Y");
                HitObstacle(obstacle);
            }
        }
    }

    private void CheckObstacleCollisionX(Vector2 position)
    {
        Vector2 obstacleRayOrigin = new Vector2(position.x, position.y);
        RaycastHit2D obstHitX = Physics2D.Raycast(obstacleRayOrigin, Vector2.right, _velocity.x * Time.fixedDeltaTime, _obstacleLayerMask);
        if (obstHitX.collider != null)
        {
            Obstacle obstacle = obstHitX.collider.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                Debug.Log("Obstacle Collision X");
                HitObstacle(obstacle);
            }
        }
    }

    private void HitObstacle(Obstacle obstacle)
    {
        obstacle.ResetObstacle();
        _velocity.x *= 0.7f;
    }

    private Vector2 UpdateJumpTimerAndYVelocity(Vector2 position)
    {
        if (_isPressingJump)
        {
            _pressJumpTimer += Time.fixedDeltaTime;
            if (_pressJumpTimer > _maxPressJumpTime)
            {
                _isPressingJump = false;
            }
        }

        position.y += _velocity.y * Time.fixedDeltaTime;
        if (!_isPressingJump)
        {
            _velocity.y += _gravity * Time.fixedDeltaTime;
        }

        return position;
    }

    private void UpdateXVelocity(Vector2 position)
    {
        float velocityRatio = _velocity.x / _maxVelocityX;
        _accelerationX = _maxAccelerationX * (1 - velocityRatio);
        _maxPressJumpTime = _maxMaxPressJumpTime * velocityRatio;

        _velocity.x += _accelerationX * Time.fixedDeltaTime;
        if (_velocity.x > _maxVelocityX)
        {
            _velocity.x = _maxVelocityX;
        }

        CheckStopGrounding(position);
    }

    private void CheckInput()
    {
        Vector2 position = transform.position;
        float groundDistance = Mathf.Abs(position.y - _groundHeight);


        if (_isGrounded || groundDistance <= _minHeightToJump)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isGrounded = false;
                _velocity.y = _jumpVelocity;
                _isPressingJump = true;
                _pressJumpTimer = 0.0f;
                Debug.Log("JUMP");
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _isPressingJump = false;
        }
    }

    private void CheckStopGrounding(Vector2 position)
    {
        Vector2 rayOrigin = new Vector2(position.x + _rayOriginOffsetX, position.y);
        Vector2 rayDirection = Vector2.up;
        float rayDistance = _velocity.y * Time.fixedDeltaTime;

        RaycastHit2D hit2D = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);

        if (hit2D.collider == null)
        {

            _isGrounded = false;
        }

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
    }
        
    private Vector2 CheckStartGrounding(Vector2 position)
    {
        Vector2 rayOrigin = new Vector2(position.x + _rayOriginOffsetX, position.y);
        Vector2 rayDirection = Vector2.up;
        float rayDistance = _velocity.y * Time.fixedDeltaTime;

        RaycastHit2D hit2D = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _groundLayerMask);

        if (hit2D.collider != null)
        {
            Ground ground = hit2D.collider.GetComponent<Ground>();
            if (ground != null)
            {
                Debug.Log("Ground Collision");

                if (position.y >= ground.GetGroundHeight())
                {
                    Debug.Log("Grounded NOW");
                    _groundHeight = ground.GetGroundHeight();
                    position.y = _groundHeight;
                    _velocity.y = 0f;
                    _isGrounded = true;
                    ground.PlayerGrounded();
                    if (ground.GetIfThisFalls())
                    {
                        _cameraShake.StartShaking();
                    }
                    else
                    {
                       _cameraShake.StopShaking();
                    }

                }

            }
        }

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);
        return position;
    }

    public int GetDistance()
    {
        return Mathf.FloorToInt(_distance);
    }

    public float GetVelocity()
    {
        return _velocity.x;
    }

    public Vector2 GetMaxJumpHeightAndWidth(Ground ground)
    {
        float h1 = _jumpVelocity * _maxPressJumpTime;
        float t = _jumpVelocity / -_gravity;
        float h2 = _jumpVelocity * t + (0.5f * (_gravity * (t * t)));

        float maxY = h1 + h2;
        maxY *= 0.7f;
        float maxJump = maxY + ground.GetGroundHeight();
        float minJump = -8f;
         float spawnY = UnityEngine.Random.Range(minJump, maxJump);



               float t1 = t + _maxPressJumpTime;
        float t2 = Mathf.Sqrt((2.0f * (maxJump - spawnY)) / -_gravity);
        float totalTime = t1 + t2;
        float maxX = totalTime * _velocity.x;

        return new Vector2(maxX , spawnY);
    }


    void ResetPlayer()
    {
        StartCoroutine(ResetPlayerPosition());
    }
    public IEnumerator ResetPlayerPosition()
    {
        yield return new WaitForSeconds(0.5f);
        transform.position = _startingPosition;
        _isDead = false;
        _distance = 0f;
        _velocity.y = 0f;
        _playerSprite.enabled = true;
    }

}
