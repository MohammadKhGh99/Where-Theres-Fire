using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFighter : MonoBehaviour
{
    [SerializeField] private int speed;
    
    private Transform _t;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private float _rightAngle = 90;
    private float _leftAngle = -90;
    private float _upAngle = 180;
    private float _downAngle = 0;
    private float _chosenAngle;
    private Vector3 _currentAngle;

    private KeyCode _right = KeyCode.RightArrow,
        _left = KeyCode.LeftArrow,
        _up = KeyCode.UpArrow,
        _down = KeyCode.DownArrow;
    

    // Start is called before the first frame update
    void Start()
    {
        _t = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody2D>();
        _moveDirection = Vector3.zero;
        _currentAngle = _t.eulerAngles;
        _chosenAngle = _t.rotation.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Lerp right and left
        
        if (Input.GetKey(_right))
        {
            _moveDirection = speed * Vector3.right;
            _chosenAngle = _rightAngle;
            // _rb.MovePosition((_rb.position + _moveDirection * Time.fixedDeltaTime));
        }
        if (Input.GetKey(_left))
        {
            _moveDirection = speed * Vector3.left;
            _chosenAngle = _leftAngle;
            // _rb.MovePosition((_rb.position + _moveDirection * Time.fixedDeltaTime));
        }
        if (Input.GetKey(_up))
        {
            _moveDirection = speed * Vector3.up;
            _chosenAngle = _upAngle;
            // _rb.MovePosition((_rb.position + _moveDirection * Time.fixedDeltaTime));
        }
        if (Input.GetKey(_down))
        {
            _moveDirection = speed * Vector3.down;
            _chosenAngle = _downAngle;
            // _rb.MovePosition((_rb.position + _moveDirection * Time.fixedDeltaTime));
        }

        _currentAngle = new Vector3(0, 0, Mathf.LerpAngle(_currentAngle.z, _chosenAngle, Time.fixedDeltaTime));
        _t.eulerAngles = _currentAngle;
        // _t.position += _moveDirection * Time.fixedDeltaTime;
        _rb.MovePosition((_rb.position + _moveDirection * Time.fixedDeltaTime));
        _moveDirection = Vector3.zero;
        // _chosenAngle = _t.rotation.z;
    }
}
