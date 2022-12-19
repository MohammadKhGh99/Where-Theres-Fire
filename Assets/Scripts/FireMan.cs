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
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool fourDirection;
    private Vector2 _moveDirection;
    
    // shooting fire
    private float _fireKeyHoldingTime = 0f;
    private bool _fireKeyDown = false;
    private float _cooldownToBomb = 0f;
    private bool _burningBuildingAnimationStarted = false;
    
    
    
    
    
    private const KeyCode Right = KeyCode.D,
                          Left = KeyCode.A,
                          Up = KeyCode.W,
                          Down = KeyCode.S;

    // private KeyCode _burnBuildingButton = KeyCode.Period;
    private const KeyCode Fire = KeyCode.Comma;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _moveDirection = Vector2.zero;
    }

    private void Update()
    {   
        // *** Movement ***
        var xDirection = Input.GetAxis("Horizontal2");
        var yDirection = Input.GetAxis("Vertical2");
        
        if (fourDirection)
            _moveDirection = GameManager.UpdateMoveDirection(_moveDirection, xDirection, yDirection);
        else
        {
            _moveDirection.x = xDirection;
            _moveDirection.y = yDirection;
            _moveDirection.Normalize();
        }


        
        // *** shooting ability ***
        if (Input.GetKeyDown(Fire))
        {
            _fireKeyDown = true;
        }
        else if (Input.GetKeyUp(Fire))
        {
            if (_fireKeyHoldingTime < 0.2f && _cooldownToBomb <= 0f)
            {
                // throwing a bomb of fire in the direction the player is looking at. (unless its a building then nothing)
                Debug.Log("Bomb");
                _cooldownToBomb = GameManager.BombCooldownTime;
                StartCoroutine(ThrowBomb());




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
                Debug.Log("torch BRRRRR");
                _fireKeyDown = false;
            }
        }
        _cooldownToBomb = Mathf.Max(_cooldownToBomb - Time.deltaTime, 0f);
        

        
    }

    IEnumerator  ThrowBomb()
    {
        var bomb = GameManager.Instance.BombPool.Get();
        // var bombT = bomb.GetComponent<Transform>();
        bomb.SetStartPosition(transform.position);
        var bombDropPos = bomb.GetBombDropPos(_moveDirection);
        yield return bomb.Shoot();
        
        // when we finish with the bomb, 

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }
}
