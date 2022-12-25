using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Molotov : MonoBehaviour
{
    // molotov
    private Transform _t;
    private Vector3 _startPos;

    // bottle
    private Transform _bottleT;
    private Vector2 _bottleScale;

    // shadow
    private Transform _shadowT;
    private Vector2 _shadowScale;
    
    // important public variables for the throw
    [SerializeField] private float molotovTravelDistance;      // how many floats the bomb should travel
    [SerializeField] private float timeToReachTarget;       // time to reach the target
    [SerializeField] private float throwingAngel;           // the angel we throw the bomb
    [SerializeField] private float scalingUpRatio;          // the scaling up ratio
    [SerializeField] private float molotovPower;               // the effect the bomb will make after hitting floor
    
    // important private variables for the throw
    private Vector2 _targetPos;
    private float _oscillationHeight;
    
    // flags for update of the throw
    private float _throwPassedTime;
    private bool _hasBeenShot = false;
    private bool _reachedTarget = false;
    private GameObject _fireChild;
    private SpriteRenderer _bottleSpriteRenderer;
    
    public void FakeStart()
    {
        _t = GetComponent<Transform>();
        _shadowT = _t.Find("Shadow");
        _bottleT = _t.Find("Bottle");
        _bottleSpriteRenderer = _bottleT.GetComponent<SpriteRenderer>();
        _shadowScale = _shadowT.localScale;
        _bottleScale = _bottleT.localScale;
        _fireChild = _t.GetChild(2).gameObject;
    }
    
    public IEnumerator Shoot(Vector3 position, Vector3 throwDirection)
    {
        // Initializing things before shooting
        _startPos = position;
        _t.position = _startPos;
        _targetPos = molotovTravelDistance * throwDirection + _startPos;

        //  shoot now!
        _hasBeenShot = true;
        _throwPassedTime = 0f;
        yield return new WaitUntil(() => _reachedTarget);
        StartCoroutine(FireLifeTime());

        // todo continue
        
    }

    public Vector3 GetMolotovDropPos()
    {
        return _targetPos;
    }

    void Update()
    {
        // shooting molotov to target
        if (_hasBeenShot && !_reachedTarget)
        {
            // calculate time until we reach the actual time to reach target
            var throwProgress = _throwPassedTime / timeToReachTarget;
            _throwPassedTime += Time.fixedDeltaTime;
            if (throwProgress >= 1)
            {
                // we reached target!
                _reachedTarget = true;
                return;
            }
            
            // get oscillation
            _oscillationHeight = Mathf.Tan(Mathf.Deg2Rad * throwingAngel) * molotovTravelDistance / 2.0f;
                
            // move position to target
            _t.position = Vector3.Lerp(_startPos, _targetPos, throwProgress);
            var aboveGroundOscillation = Mathf.Sin(throwProgress * Mathf.PI) * _oscillationHeight;
            _bottleT.localPosition = new Vector3(0,aboveGroundOscillation * _bottleScale.y,0);        //todo maybe += instead of =
            
            // enlarging the bottle
            var scalingUpMaxBottle = scalingUpRatio * _bottleScale;
            var bottleNewScale = Vector2.zero;
            bottleNewScale.x = _bottleScale.x + Mathf.Sin(throwProgress * Mathf.PI) * scalingUpMaxBottle.x;
            bottleNewScale.y = _bottleScale.y + Mathf.Sin(throwProgress * Mathf.PI) * scalingUpMaxBottle.y;
            _bottleT.localScale = bottleNewScale;
            
            // enlarging the shadow
            var scalingUpMaxShadow = scalingUpRatio * _bottleScale;
            var shadowNewScale = Vector2.zero;
            shadowNewScale.x = _shadowScale.x + Mathf.Sin(throwProgress * Mathf.PI) * scalingUpMaxShadow.x;
            shadowNewScale.y = _shadowScale.y;
            _shadowT.localScale = shadowNewScale;

        }
    }

    private IEnumerator FireLifeTime()
    {
        _fireChild.SetActive(true);
        _bottleSpriteRenderer.sprite = null;
        yield return new WaitForSeconds(8);
        gameObject.SetActive(false);
    }
    
}
