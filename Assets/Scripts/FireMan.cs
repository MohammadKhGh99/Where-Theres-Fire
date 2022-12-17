using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMan : MonoBehaviour
{
    [SerializeField] private int speed;

    private Transform _t;
    private Vector3 _moveDirection;
    private Rigidbody2D _rb;
    private float _rightAngle = 90;
    private float _leftAngle = -90;
    private float _upAngle = 180;
    private float _downAngle = 0;
    private float _chosenAngle;
    private Vector3 _currentAngle;
    
    private KeyCode _right = KeyCode.D,
        _left = KeyCode.A,
        _up = KeyCode.W,
        _down = KeyCode.S;
    
    // Start is called before the first frame update
    void Start()
    {
        _t = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody2D>();
        _moveDirection = Vector3.zero;
        _chosenAngle = _t.rotation.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Lerp right and left

        if (Input.GetKey(_right))
        {
            _moveDirection = Vector3.right;
            _chosenAngle = _rightAngle;
        }
        if (Input.GetKey(_left))
        {
            _moveDirection = Vector3.left;
            _chosenAngle = _leftAngle;
        }
        if (Input.GetKey(_up))
        {
            _moveDirection = Vector3.up;
            _chosenAngle = _upAngle;
        }
        if (Input.GetKey(_down))
        {
            _moveDirection = Vector3.down;
            _chosenAngle = _downAngle;
        }

        // _currentAngle = new Vector3(0, 0, Mathf.LerpAngle(_currentAngle.z, _chosenAngle, Time.fixedDeltaTime));
        // _rb.MovePosition((_t.position + _moveDirection * (speed * Time.fixedDeltaTime)).normalized);
        _moveDirection = Vector3.zero;
        _chosenAngle = _t.rotation.z;
    }
}
