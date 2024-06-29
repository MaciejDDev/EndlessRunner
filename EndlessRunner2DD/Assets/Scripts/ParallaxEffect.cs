using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField] int _depth;
    static float _spawningXValue = -45f;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        float realVelocity = Player.Instance.GetVelocity() / _depth;
        Vector2 position = transform.position;

        position.x -= realVelocity * Time.deltaTime;
    
        if (position.x < _spawningXValue) 
        {
            position.x = -_spawningXValue;
        }

        transform.position = position;
    }
}
