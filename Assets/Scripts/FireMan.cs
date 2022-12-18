using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireMan : MonoBehaviour
{
    private Transform _t;
    private Rigidbody2D _rb;

    // movement
    [SerializeField] private int movingSpeed;
    private Vector2 _moveDirection;
    
    
    private KeyCode _right = KeyCode.D,
                    _left = KeyCode.A,
                    _up = KeyCode.W,
                    _down = KeyCode.S;
    
    // Start is called before the first frame update
    void Start()
    {
        _t = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody2D>();
        _moveDirection = Vector2.zero;
    }

    private void Update()
    {
        var xDirection = Input.GetAxis("Horizontal2");
        var yDirection = Input.GetAxis("Vertical2");
        
        // if 4 Directions
        // _moveDirection = GameManager.UpdateMoveDirection(_moveDirection, xDirection, yDirection);
        
        // else
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        _moveDirection.Normalize();
        // float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);
    }
    
    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }
}
