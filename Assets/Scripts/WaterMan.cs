using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMan : MonoBehaviour
{
    // components
    private Transform _t;
    private Rigidbody2D _rb;
    
    // movement
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool fourDirection;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;
    
    // shooting fire
    private float _fireKeyHoldingTime = 0f;
    private bool _fireKeyDown = false;
    private float _cooldownToWaterGun = 0f;
    private bool _burningBuildingAnimationStarted = false;
    
    // controls changing
    private const KeyCode Fire = KeyCode.Period;
    private const KeyCode Right = KeyCode.RightArrow,
                          Left = KeyCode.LeftArrow,
                          Up = KeyCode.UpArrow,
                          Down = KeyCode.DownArrow;

    

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _lookAtDirection = Vector2.left;
    }

    private void Update()
    {
        // *** Movement ***
        var xDirection = Input.GetAxis("Horizontal1");
        var yDirection = Input.GetAxis("Vertical1");
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            transform.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _lookAtDirection = _moveDirection;
        }
        
        // *** shooting ability ***
        if (Input.GetKeyDown(Fire))
        {
            _fireKeyDown = true;
        }
        else if (Input.GetKeyUp(Fire))
        {
            if (_fireKeyHoldingTime < 0.2f && _cooldownToWaterGun <= 0f)
            {
                // throwing a bomb of fire in the direction the player is looking at. (unless its a building then nothing)
                Debug.Log("Water Splash");
                _cooldownToWaterGun = GameManager.SplashBulletCooldownTime;
                StartCoroutine(ThrowSplashBullet());

            }

            _fireKeyHoldingTime = 0f;
            _fireKeyDown = false;
        }
        
        if (_fireKeyDown)
        {
            _fireKeyHoldingTime += Time.deltaTime;
            if (_fireKeyHoldingTime >= 1f && !_burningBuildingAnimationStarted)
            {
                // start burning building animation 
                _burningBuildingAnimationStarted = true;
            }
            if (_fireKeyHoldingTime >= 5f)
            {
                // the torch is thrown in the -building- and it will start to burn - stop animation also
                Debug.Log("Water is everywhere! (5 sec)");
                _fireKeyDown = false;
            }
        }
        _cooldownToWaterGun = Mathf.Max(_cooldownToWaterGun - Time.deltaTime, 0f);
        
    }
    
    private IEnumerator ThrowSplashBullet()
    {
        var splashBullet = GameManager.instance.SplashBulletPool.Get();
        yield return splashBullet.Shoot(transform.position, _lookAtDirection);
        // when we finish with the bomb, 
        var molotovDropPos = splashBullet.GetSplashBulletDropPos();

        

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }
}