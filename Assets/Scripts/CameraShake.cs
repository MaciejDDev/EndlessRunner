using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    [SerializeField] float _shakeDistance = 0.2f;
    [SerializeField] float _shakeSpeedY = 1f;
    [SerializeField] float _shakeSpeedX = 1f;

    Vector3 _startingPosition;
    Vector3 _shakeOffset;
    private bool _isShaking;


    // Start is called before the first frame update
    void Start()
    {
        _startingPosition = transform.position;
        _isShaking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isShaking)
        {
            Vector3 currentpos = transform.position;
            Vector3 offsetPos = currentpos + _shakeOffset;
            float currentDistanceY = offsetPos.y - _startingPosition.y;
            if (Mathf.Abs(currentDistanceY) > _shakeDistance)
            {
                _shakeSpeedY *= -1;
            }

            _shakeOffset.y += _shakeSpeedY * Time.deltaTime;

            if (_shakeOffset.y > _shakeDistance)
                _shakeOffset.y = _shakeDistance;
            if (_shakeOffset.y < -_shakeDistance)
                _shakeOffset.y = -_shakeDistance;

            float currentDistanceX = offsetPos.x - _startingPosition.x;
            if (Mathf.Abs(currentDistanceX) > _shakeDistance)
            {
                _shakeSpeedX *= -1;
            }

            _shakeOffset.x += _shakeSpeedX * Time.deltaTime;

            if (_shakeOffset.x > _shakeDistance)
                _shakeOffset.x = _shakeDistance;
            if (_shakeOffset.x < -_shakeDistance)
                _shakeOffset.x = -_shakeDistance;



            transform.position = _startingPosition + _shakeOffset;

        }
    }


    public void StartShaking()
    {
        _isShaking = true;
    }

    public void StopShaking()
    {
        _isShaking = false;
    }

}
