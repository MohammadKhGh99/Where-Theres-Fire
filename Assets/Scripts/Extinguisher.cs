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
    [SerializeField] private Transform waterGunLocalPos;
    private WaterGun _waterGun;
    private float _waterKeyHoldingTime = 0f;
    private bool _waterKeyDown = false;
    
    // controls changing
    private const KeyCode Extinguish = KeyCode.Period;
    
    // ** water particle
    private ParticleSystem _waterSplash;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _waterGun = GetComponent<WaterGun>();
        _waterSplash = _t.GetChild(0).GetComponent<ParticleSystem>();
        _lookAtDirection = Vector2.left;
    }

    private void Update()
    {
        // *** Movement ***
        var yDirection = Input.GetAxis("Vertical");
        var xDirection = Input.GetAxis("Horizontal");
        
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            _isMoving = true;
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            transform.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            // _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _moveDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            _lookAtDirection = _moveDirection;
        }
        else
        {
            _isMoving = false;
        }
        
        
        if (_waterKeyDown)      // we are holding the water key
        {
            _waterKeyHoldingTime += Time.deltaTime;
            // TODO: here we spray particles as long as we keep pressing the button
            // PARTICLE!
            _waterSplash.Play();
            _waterGun.EnlargeWaterStream(_t.position, _lookAtDirection, waterGunLocalPos.position);
        }
        
        // *** shooting ability ***
        if (Input.GetKeyDown(Extinguish))   // we started clicking the button
        {
            _waterKeyDown = true;
            
            // create a new waterStream
            _waterGun.CreateWaterStream();
        }
        else if (Input.GetKeyUp(Extinguish))    // we stopped clicking the button
        {
            _waterSplash.Stop();
            _waterGun.ShootWaterStream();
            _waterKeyHoldingTime = 0f;
            _waterKeyDown = false;
            
            // stop enlarging the waterStream - shoot it 
        }
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // _rb.MovePosition(_rb.position + _moveDirection * (35.0f * Time.fixedDeltaTime));
        _rb.velocity = _moveDirection.normalized * (movingSpeed * Time.fixedDeltaTime);
    }
}