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
    [SerializeField] private Vector3 waterGunLocalPosVec = new Vector3(0, -1.25f, 0);
    
    // controls changing
    private const KeyCode Extinguish = KeyCode.Period;
    
    // ** water particle
    private ParticleSystem _waterSplash;

    private Vector3 _startPosition;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _waterGun = GetComponent<WaterGun>();
        _waterSplash = _t.GetChild(0).GetComponent<ParticleSystem>();
        _lookAtDirection = Vector2.left;
        _startPosition = _t.position;
    }

    private void Update()
    {
        // don't move when the game is not started yet!!!
        if (!GameManager.IsGameRunning)
            return;
        
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
            // transform.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            // _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _moveDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            _lookAtDirection = _moveDirection;
        }
        else
        {
            _isMoving = false;
        }
        
        
        // *** shooting ability ***
        if (Input.GetKey(Extinguish))   // we started holding the button
        {
            _waterSplash.Play();
            _waterGun.EnlargeWaterStream(_t.position, _lookAtDirection, waterGunLocalPos.position, _waterKeyDown);
            _waterKeyDown = true;
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
    
    public void StartGame()
    {
        _t.position = _startPosition;
    }
}