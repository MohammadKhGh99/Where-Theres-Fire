using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Rigidbody2D _rb;

    // bomb ability
    [SerializeField] private float bombTravelDistance;      // how many floats the bomb should travel
    [SerializeField] private float timeToReachTarget;       // time to reach the target
    [SerializeField] private float throwingAngel;       // time to reach the target
    [SerializeField] private float bombPower;       // the effect the bomb will make
    private Vector3 _startPos;
    private Vector3 _targetPos;
    
    // Shadow vars
    private Transform _shadowT;         //  shadow transform
    private Vector3 _shadowStartScale;           //  the shadow scale started

    // to make the bomb looks like its levitating up, (thrown in like 45 degrees)
    private float _oscillationHeight;

    // this how update works
    private float _throwStartTime;
    private bool _hasBeenShot = false;
    private bool _reachedTarget = false;
    
    
    public void FakeStart()
    {
        _rb = GetComponent<Rigidbody2D>();
        _shadowT = transform.Find("Shadow");
    }
    private void Update()
    {
        if (_hasBeenShot && !_reachedTarget)
        {
            // calculate time until we reach the actual time to reach target
            var timePassed = Time.time - _throwStartTime;
            var throwProgress = timePassed / timeToReachTarget;
            if (throwProgress >= 1)
            {
                _reachedTarget = true;
                return;
            }
            
            // get oscillation
            _oscillationHeight = Mathf.Tan(Mathf.Deg2Rad * throwingAngel) * bombTravelDistance / 2.0f;
                
            // move position to target
            var bombPosition = Vector3.Lerp(_startPos, _targetPos, throwProgress);
            var aboveGroundOscillation = Mathf.Sin(throwProgress * Mathf.PI) * _oscillationHeight;
            bombPosition.y += aboveGroundOscillation;
            // _rb.MovePosition(bombPosition);
            transform.Translate(bombPosition);
            _shadowT.localPosition += new Vector3(0,-1 * aboveGroundOscillation,0);

            // var bombScale = 1 + Mathf.Abs(Mathf.Sin(_oscillationPhase * Mathf.PI * 2)) * 0.5f;
            // transform.localScale = Vector3.one * bombScale;

            // Calculate the scale of the shadow based on the oscillation phase
            // float shadowScale = _shadowStartScale.x + Mathf.Abs(Mathf.Sin(_oscillationPhase * Mathf.PI * 2)) * 0.5f;

            // Set the scale of the shadow
            // _shadowT.transform.localScale = new Vector3(shadowScale, _shadowStartScale.y, shadowScale);
        }
    }

    public Vector3 GetBombDropPos()
    {
        return _targetPos;
    }
    
    public IEnumerator Shoot(Vector3 position, Vector3 throwDirection)
    {
        _startPos = position;
        transform.position = _startPos;
        _targetPos = bombTravelDistance * throwDirection + _startPos;
        _shadowStartScale = _shadowT.transform.localScale;
        
        _hasBeenShot = true;
        _throwStartTime = Time.time;
        
        yield return new WaitUntil(() => _reachedTarget);
        _reachedTarget = false;
    }
}
