using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class FireMan : MonoBehaviour
{
    // Game manager
    private Transform _t;
    private Rigidbody2D _rb;

    // movement
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool moveByGrid = true;
    [SerializeField] private bool fourDirection = true;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;

    // grid movement
    [SerializeField] private float gridMoveDuration;
    [SerializeField] private int numGridMove = 2;
    private Vector3Int _currentGridPos; // current position of the object in grid cells
    private float _gridMoveTimer; // timer for moving from one grid cell to the next

    // shooting fire
    private float _fireKeyHoldingTime;
    private bool _fireKeyDown;
    private float _cooldownToMolotov;
    private bool _burningBuildingAnimationStarted;

    // controls changing
    private const KeyCode Fire = KeyCode.T;
    private const KeyCode Up = KeyCode.W, Down = KeyCode.S, Left = KeyCode.A, Right = KeyCode.D;

    private RaycastHit2D _hit;
    private LayerMask _buildingsMask;
    
    //**Animation**
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _lookAtDirection = Vector2.right;
        _currentGridPos = GameManager.Instance.GroundBaseTilemap.WorldToCell(transform.position);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // don't move when the game is not started yet!!!
        if (!GameManager.IsGameRunning)
            return;

        // *** Movement ***
        var xDirection = Input.GetAxisRaw("Horizontal2");
        var yDirection = Input.GetAxisRaw("Vertical2");
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        if(!moveByGrid)
        {
            SnappingMovement();
        }
        else
        {
            GridMovement();
        }
        

        // *** shooting ability ***
        if (Input.GetKeyDown(Fire))
        {
            _fireKeyDown = true;
        }
        else if (Input.GetKeyUp(Fire))
        {
            if (_fireKeyHoldingTime < 0.2f && _cooldownToMolotov <= 0f)
            {
                // throwing a bomb of fire in the direction the player is looking at. (unless its a building then nothing)
                _cooldownToMolotov = GameManager.MolotovCooldownTime;
                StartCoroutine(ThrowMolotov());
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
                print("FIIRREEE BRRRRR");
                _fireKeyDown = false;
            }
        }
        _cooldownToMolotov = Mathf.Max(_cooldownToMolotov - Time.deltaTime, 0f);
    }


    private void SnappingMovement()
    {
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            // _t.rotation = Quaternion.AngleAxis(90 + angle, Vector3.forward);
            _moveDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
            _lookAtDirection = _moveDirection;
        }
    }
    
    private void GridMovement()
    {
        // Update the timer
        // _gridMoveTimer += Time.deltaTime;

        // Check if it's time to move to the next grid cell
        // if (_gridMoveTimer >= gridMoveDuration)
        // {
        // Reset the timer
        // _gridMoveTimer = 0;
        // _lookAtDirection = Vector3.zero;
        
        // Check input and move in the corresponding direction
        if (Input.GetKeyDown(Up))
        {
            _lookAtDirection = Vector3.up;
            _hit = Physics2D.Raycast(_t.position, _lookAtDirection, 1, layerMask: GameManager.Instance.HousesMask | GameManager.Instance.BordersMask);
            if(!_hit)
                _currentGridPos.y += numGridMove;
        }
        else if (Input.GetKeyDown(Down))
        {
            _lookAtDirection = Vector3.down;
            _hit = Physics2D.Raycast(_t.position, _lookAtDirection, 1, layerMask: GameManager.Instance.HousesMask | GameManager.Instance.BordersMask);
            if(!_hit)
                _currentGridPos.y -= numGridMove;
        }
        else if (Input.GetKeyDown(Right))
        {
            _lookAtDirection = Vector3.right;
            _hit = Physics2D.Raycast(_t.position, _lookAtDirection, 1, layerMask: GameManager.Instance.HousesMask | GameManager.Instance.BordersMask);
            if(!_hit)
                _currentGridPos.x += numGridMove;
            if (_spriteRenderer.flipX)
                _spriteRenderer.flipX = false;
        }
        else if (Input.GetKeyDown(Left))
        {
            _lookAtDirection = Vector3.left;
            _hit = Physics2D.Raycast(_t.position, _lookAtDirection, 1, layerMask: GameManager.Instance.HousesMask | GameManager.Instance.BordersMask);
            if(!_hit)
                _currentGridPos.x -= numGridMove;
            if (!_spriteRenderer.flipX)
                _spriteRenderer.flipX = true;
        }
        _animator.SetInteger("XSpeed", (int)_lookAtDirection.x);
        _animator.SetInteger("YSpeed", (int)_lookAtDirection.y);
        StartCoroutine(DelayForFireManMovement());
        
        // Update the position of the object in world space
        transform.position = GameManager.Instance.GroundBaseTilemap.GetCellCenterWorld(_currentGridPos);
        // }
    }

    private IEnumerator DelayForFireManMovement()
    {
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ThrowMolotov()
    {
        var molotov = GameManager.Instance.MolotovPool.Get();
        yield return molotov.Shoot(_t.position, _lookAtDirection);
        // when we finish with the bomb, 
        var molotovDropPos = molotov.GetMolotovDropPos();
        GameManager.Instance.MolotovPool.Release(molotov);
        StartCoroutine(StartFire(molotovDropPos));
    }

    private IEnumerator StartFire(Vector3 molotovDropPos)
    {
        var checkWhereDropCollider2D = Physics2D.OverlapPoint(molotovDropPos, layerMask: GameManager.Instance.HousesMask);
        
        if (checkWhereDropCollider2D.IsUnityNull())
        {
            // we will drop the molotovFire on street
        }
        else
        {
            // we will drop molotov fire at Building
            var building = checkWhereDropCollider2D.GetComponent<HouseManager>();
            molotovDropPos = building.GetBuildingPos();
            building.SetStatus(GameManager.HouseStatus.Burning);
        }

        var molotovFire = GameManager.Instance.FireMolotovPool.Get();
        molotovFire.Burn(molotovDropPos);
        
        yield break;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(!moveByGrid)
            _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }
}