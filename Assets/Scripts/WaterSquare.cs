using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSquare : MonoBehaviour
{
    // Spray
    private Transform _t;
    private Rigidbody2D _rb;
    private Vector3 _startPos;
    
    // important public variables for the throw
    [SerializeField] private float travelDistance;                       // how many floats the water stream should travel
    [SerializeField] private float travelingTime = 5.0f;                 // Square life until dying
    [SerializeField] private float power;                                // the force it will slap the player with (used In Collision)

    // important private variables for the throw
    private Vector3 _diePosition;
    
    // flags for update of the throw
    private bool _hasBeenShot;
    private bool _reachedTarget;
    private float _throwPassedTime;

    public void FakeStart()
    {
        // what do you want to happen when we "GET" the object to the pool
        
        _t = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody2D>();
        gameObject.SetActive(true);
        _t.rotation = Quaternion.identity;
        _hasBeenShot = false;
        _reachedTarget = false;
    }
    
    public void FakeRelease()
    {
        // what do you want to happen when we "RETURN/RELEASE" the object to the pool
        gameObject.SetActive(false);
    }
    
    
    public IEnumerator Shoot(Vector3 position, Vector3 sprayDirection)
    {
        // Initializing things before shooting
        _startPos = position;
        _t.position = _startPos;
        _diePosition = travelDistance * sprayDirection + _startPos;

        //  shoot now!
        _hasBeenShot = true;
        _throwPassedTime = 0;
        
        // todo continue
        yield return new WaitUntil(() => _reachedTarget);
    }
    
    public Vector3 GetWaterSquareDiePos()
    {
        // this is useful because after it die, we need to make the street wet. (or wherever it drops)
        return _diePosition;
    }
    
    void FixedUpdate()
    {
        // shooting molotov to target
        if (_hasBeenShot && !_reachedTarget)
        {
            // calculate time until we reach the actual time to reach target
            var throwProgress = _throwPassedTime / travelingTime;
            _throwPassedTime += Time.fixedDeltaTime;
            if (throwProgress >= 1)
            {
                // we reached target!
                _reachedTarget = true;
                return;
            }

            // move position to target
            var newPos = Vector3.Lerp(_startPos, _diePosition, throwProgress);
            _rb.MovePosition(newPos);
        }
    }

    
    public (Vector3, float, float) GetWaterSquareInformation()
    {
        // this used in watergun class, just once
        return (_t.localScale, travelDistance, travelingTime);
    }
}
