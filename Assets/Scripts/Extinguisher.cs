using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : MonoBehaviour
{
    // components
    private Transform _t;
    private Rigidbody2D _rb;
    
    // movement
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool fourDirection;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;
    private bool _isMoving;
    
    // shooting water
    private WaterGun _waterGun;
    private Vector2 _waterGunLocalPos;
    private float _waterKeyHoldingTime = 0f;
    private bool _waterKeyDown = false;
    
    // controls changing
    private const KeyCode Extinguish = KeyCode.Period;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _waterGun = GetComponent<WaterGun>();
        _waterGunLocalPos = new Vector2(0.25f, -1.5f);
        _lookAtDirection = Vector2.left;
    }

    private void Update()
    {
        // *** Movement ***
        var xDirection = Input.GetAxis("Horizontal");
        var yDirection = Input.GetAxis("Vertical");
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            _isMoving = true;
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            transform.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _lookAtDirection = _moveDirection;
        }
        else
        {
            // float angle = Vector2.Angle(_lookAtDirection, Vector2.right);
            // transform.rotation = Quaternion.Euler( 0,0, angle);
        }
        
        
        
        // *** shooting ability ***
        if (Input.GetKeyDown(Extinguish))   // we started clicking the button
        {
            _waterKeyDown = true;
        }
        else if (Input.GetKeyUp(Extinguish))    // we stopped clicking the button
        {
            _waterKeyHoldingTime = 0f;
            _waterKeyDown = false;
        }
        
        if (_waterKeyDown)      // we are holding the water key
        {
            _waterKeyHoldingTime += Time.deltaTime;
            // TODO: here we spray particles as long as we keep pressing the button
            // PARTICLE!
            
            if (_waterKeyHoldingTime >= 0.2f)
            {
                // here we actually shoot water (invisible) as far as we are pressing the button
                
                //SHOOT
                var waterSquarePosWorld = _t.TransformPoint(_waterGunLocalPos);
                StartCoroutine(_waterGun.Shoot(waterSquarePosWorld, _lookAtDirection));
            }
        }
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }
}