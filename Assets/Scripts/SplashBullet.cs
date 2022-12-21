using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashBullet : MonoBehaviour
{
    // Splash
    private Transform _t;
    private Vector3 _startPos;
    private Vector2 _startScale;
    
    // important public variables for the throw
    [SerializeField] private float splashBulletTravelDistance;      // how many floats the bomb should travel
    [SerializeField] private float timeToReachTarget;       // time to reach the target
    [SerializeField] private float splashBulletPower;    
    
    // important private variables for the throw
    private Vector2 _targetPos;
    
    // flags for update of the throw
    private float _throwStartTime;
    private bool _hasBeenShot = false;
    private bool _reachedTarget = false;
    
    public void FakeStart()
    {
        _t = GetComponent<Transform>();
        _startScale = _t.localScale;
    }
    
    
    public IEnumerator Shoot(Vector3 position, Vector3 throwDirection)
    {
        // Initializing things before shooting
        _startPos = position;
        _t.position = _startPos;
        _targetPos = splashBulletTravelDistance * throwDirection + _startPos;
        
        // fix rotation
        var angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle -45 + 180, Vector3.forward);
        
        //  shoot now!
        _hasBeenShot = true;
        _throwStartTime = Time.time;
        yield return new WaitUntil(() => _reachedTarget);
        
        // todo continue
    }
    
    public Vector3 GetSplashBulletDropPos()
    {
        return _targetPos;
    }
    
 
    void Update()
    {
        // shooting molotov to target
        if (_hasBeenShot && !_reachedTarget)
        {
            // calculate time until we reach the actual time to reach target
            var timePassed = Time.time - _throwStartTime;
            var throwProgress = timePassed / timeToReachTarget;
            if (throwProgress >= 1)
            {
                // we reached target!
                _reachedTarget = true;
                return;
            }

            // move position to target
            _t.position = Vector3.Lerp(_startPos, _targetPos, throwProgress);
        }
    }
}
