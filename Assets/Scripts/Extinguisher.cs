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
    private float _waterKeyHoldingTime;
    private bool _waterKeyDown;

    // controls changing
    private const KeyCode Extinguish = KeyCode.Period;

    // ** water particle
    private ParticleSystem _waterSplash;

    private Vector3 _startPosition;

    // ** Animations **
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _throwDirection;

    // ** rotating and positioning particle system
    private Quaternion _leftAngle = Quaternion.identity;
    private Quaternion _rightAngle = Quaternion.Euler(0, 0, 180);
    private Quaternion _downAngle = Quaternion.Euler(0, 0, 90);
    private Quaternion _upAngle = Quaternion.Euler(0, 0, 270);
    private Vector3 _leftPos = new(-0.4f, 0.4f, 0);
    private Vector3 _rightPos = new(0.6f, 0.4f, 0);
    private Vector3 _upPos = Vector3.up;

    private float _prevRotation;
    private Vector3 _prevWaterSplashPos;

    // private Vector2 _lookAtDirection;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _waterGun = GetComponent<WaterGun>();
        _waterSplash = _t.GetChild(0).GetComponent<ParticleSystem>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _lookAtDirection = Vector2.left;
        _startPosition = _t.position;
        _moveDirection = _startPosition;
        _throwDirection = Vector2.left;
        _prevWaterSplashPos = _startPosition + 0.2f * Vector3.up + 0.2f * Vector3.right;
    }

    private void Update()
    {
        // don't move when the game is not started yet!!!
        if (!GameManager.IsGameRunning)
            return;

        // *** Movement ***
        var yDirection = Input.GetAxis("Vertical");
        var xDirection = Input.GetAxis("Horizontal");
        // print(xDirection + " " + yDirection);

        // if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && 
        //     !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
        // {
        //     _moveDirection = Vector2.zero;
        // }
        // else
        // {
        //     _throwDirection.x = xDirection;
        //     _throwDirection.y = yDirection;
        //     
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        // }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // if (!_throwDirection.x.Equals(-1) && _waterSplash.isPlaying)
                // _waterSplash.Stop();
            
            _throwDirection.x = -1;
            _throwDirection.y = 0;
            if (_spriteRenderer.flipX)
                _spriteRenderer.flipX = false;
            var temp = _waterSplash.transform;
            temp.position = _t.position + _leftPos;
            temp.rotation = _leftAngle;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            // if (!_throwDirection.x.Equals(1) && _waterSplash.isPlaying)
                // _waterSplash.Stop();
            
            _throwDirection.x = 1;
            _throwDirection.y = 0;
            if (!_spriteRenderer.flipX)
                _spriteRenderer.flipX = true;
            var temp = _waterSplash.transform;
            temp.position = _t.position + _rightPos;
            temp.rotation = _rightAngle;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            // if (!_throwDirection.y.Equals(-1) && _waterSplash.isPlaying)
                // _waterSplash.Stop();
            
            _throwDirection.x = 0;
            _throwDirection.y = -1;
            var temp = _waterSplash.transform;
            temp.position = _t.position;
            temp.rotation = _downAngle;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            // if (!_throwDirection.y.Equals(1) && _waterSplash.isPlaying)
            //     _waterSplash.Stop();
            
            _throwDirection.x = 0;
            _throwDirection.y = 1;
            var temp = _waterSplash.transform;
            temp.position = _t.position + _upPos;
            temp.rotation = _upAngle;
        }
        else if (!Input.GetKey(Extinguish))
        {
            _moveDirection = Vector2.zero;
            _throwDirection = Vector2.zero;
        }

        _animator.SetInteger("XSpeed", (int)_throwDirection.x);
        _animator.SetInteger("YSpeed", (int)_throwDirection.y);

        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            // RotatingAndPositioningWaterStream();

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
        if (Input.GetKey(Extinguish)) // we started holding the button
        {
            // _prevWaterSplashPos = _waterSplash.transform.position;
            if (_waterSplash.isStopped)
                _waterSplash.Play();
            
            if (!GameManager.Instance.GetWaterHoseSound().isPlaying)
                GameManager.Instance.GetWaterHoseSound().Play();
            
            _waterGun.EnlargeWaterStream(_t.position, _lookAtDirection, waterGunLocalPos.position, _waterKeyDown);
            _waterKeyDown = true;
        }
        else if (Input.GetKeyUp(Extinguish)) // we stopped clicking the button
        {
            if (_waterSplash.isPlaying)
                _waterSplash.Stop();
            
            if (GameManager.Instance.GetWaterHoseSound().isPlaying)
                GameManager.Instance.GetWaterHoseSound().Stop();
            
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

    private void RotatingAndPositioningWaterStream()
    {
        switch (_moveDirection.x)
        {
            // moving right
            case > 0:
            {
                var temp = _waterSplash.transform;
                temp.position = _t.position + _rightPos;
                temp.rotation = _rightAngle;
                break;
            }
            // moving left
            case < 0:
            {
                var temp = _waterSplash.transform;
                temp.position = _t.position + _leftPos;
                temp.rotation = _leftAngle;
                break;
            }
            default:
                switch (_moveDirection.y)
                {
                    // moving down
                    case < 0:
                    {
                        var temp = _waterSplash.transform;
                        temp.position = _t.position;
                        temp.rotation = _downAngle;
                        break;
                    }
                    // moving up
                    case > 0:
                    {
                        var temp = _waterSplash.transform;
                        temp.position = _t.position + _upPos;
                        temp.rotation = _upAngle;
                        break;
                    }
                }

                break;
        }
    }

    public void StartGame()
    {
        _t.position = _startPosition;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name.Equals("FireMan"))
        {
            // print("HEllpasd");
            col.gameObject.GetComponent<FireMan>().GetHideable().ShowOrHide(reShow: true);
        }
    }
}