using System;
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
    [SerializeField] private float lifeTime = 5.0f;
    
    // important private variables for the throw
    private Vector2 _targetPos;
    
    // flags for update of the throw
    private float _throwStartTime;
    private bool _hasBeenShot = false;
    private bool _reachedTarget = false;
    private bool _watering;
    
    public void FakeStart()
    {
        _t = GetComponent<Transform>();
        // GetComponent<SpriteRenderer>().sprite = null;
        _startScale = _t.localScale;
        _watering = false;
    }
    
    
    public IEnumerator Shoot(Vector3 position, Vector3 throwDirection)
    {
        // Initializing things before shooting
        _startPos = position;
        _t.position = _startPos;
        _targetPos = splashBulletTravelDistance * throwDirection + _startPos;
        
        // fix rotation
        var angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        _t.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
        
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

    public void Die()
    {
        gameObject.SetActive(false);
    }
    
 
    void Update()
    {
        // shooting water to target
        if (_watering)
            _t.localPosition = Vector3.down * 8;
        if (_hasBeenShot && !_reachedTarget && !_watering)
        {
            // calculate time until we reach the actual time to reach target
            var timePassed = Time.time - _throwStartTime;
            var throwProgress = timePassed / timeToReachTarget;
            // if (throwProgress >= 1)
            // {
            //     // we reached target!
            //     _reachedTarget = true;
            //     return;
            // }
            // print("Moved!");
            
            // move position to target
            _t.position = Vector3.Lerp(_t.position, _targetPos, throwProgress);
        }

        if (_reachedTarget && !_watering)
            _watering = true;
    }

    public void SetWatering(bool other)
    {
        _watering = other;
    }

    // private void OnCollisionEnter2D(Collision2D col)
    // {
    //     print(col.gameObject);
    // }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // print(col.gameObject);
        if (col.gameObject.name.StartsWith("Building"))
        {
            col.GetComponent<BuildingManager>().SetStatus(GameManager.BURNING);
        }

        if (col.gameObject.name.EndsWith("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name.StartsWith("Building"))
        {
            other.GetComponent<BuildingManager>().SetStatus(GameManager.WAS_BURNED);
        }
    }
}
