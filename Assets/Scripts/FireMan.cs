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
    [SerializeField] private bool moveByGrid;
    [SerializeField] private bool fourDirection;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;

    // grid movement
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Vector3Int gridSize;
    [SerializeField] private float gridMoveDuration;
    private Vector3Int currentGridPos; // current position of the object in grid cells
    private float gridMoveTimer; // timer for moving from one grid cell to the next
    
    // shooting fire
    private float _fireKeyHoldingTime = 0f;
    private bool _fireKeyDown = false;
    private float _cooldownToMolotov = 0f;
    private bool _burningBuildingAnimationStarted = false;

    // hiding ability 
    [SerializeField] private bool invisible;
    private Sprite _mySprite;
    private SpriteRenderer _spriteRenderer;
    private bool _shown;
    private float _hideTime;

    // controls changing
    private const KeyCode Fire = KeyCode.T;
    private const KeyCode Right = KeyCode.D,
                          Left = KeyCode.A,
                          Up = KeyCode.W,
                          Down = KeyCode.S;
 
    private RaycastHit2D _hit;
    private LayerMask _buildingsMask;
    private Vector3 _startPosition;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _startPosition = _t.position;
        _lookAtDirection = Vector2.right;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (invisible)
        {
            _spriteRenderer.enabled = false;
            // _mySprite = _spriteRenderer..sprite;
            // _spriteRenderer.sprite = null;
            _shown = false;
            _hideTime = 4.0f;
        }
    }

    private void Update()
    {
        // don't move when the game is not started yet!!!
        if (!GameManager.IsGameRunning)
            return;
        
        // ** make Invisible **
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(_spriteRenderer.enabled
                ? GameManager.Instance.FadeOut(_spriteRenderer)
                : GameManager.Instance.FadeIn(_spriteRenderer));
            // _spriteRenderer.enabled = !_spriteRenderer.enabled;
        }
        
        // *** Movement ***
        if(!moveByGrid)
        {
            var xDirection = Input.GetAxisRaw("Horizontal2");
            var yDirection = Input.GetAxisRaw("Vertical2");
            _moveDirection.x = xDirection;
            _moveDirection.y = yDirection;

            var snapping = fourDirection ? 90.0f : 45.0f;
            if (_moveDirection.sqrMagnitude > 0)
            {
                var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
                angle = Mathf.Round(angle / snapping) * snapping;
                _t.rotation = Quaternion.AngleAxis(90 + angle, Vector3.forward);
                _moveDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                _lookAtDirection = _moveDirection;
            }
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
        
        // *** Power-Up: hide the character ***
        // if (_shown)
        // {
        //     _hideTime -= Time.deltaTime;
        //     if (_hideTime <= 0)
        //     {
        //         _hideTime = 4.0f;
        //         _spriteRenderer.enabled = false;
        //         // _spriteRenderer.sprite = null;
        //         _shown = false;
        //     }
        // }
    }

    private void GridMovement()
    {
        // Update the timer
        gridMoveTimer += Time.deltaTime;

        // Check if it's time to move to the next grid cell
        if (gridMoveTimer >= gridMoveDuration)
        {
            // Reset the timer
            gridMoveTimer = 0;

            // Check input and move in the corresponding direction
            if (Input.GetKeyDown(KeyCode.W))// && currentGridPos.y < gridSize.y - 1)
            {
                currentGridPos.y++;
                _moveDirection.x = 0;
                _moveDirection.y = 1;

                var snapping = 90.0f;
                if (_moveDirection.sqrMagnitude > 0)
                {
                    var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
                    angle = Mathf.Round(angle / snapping) * snapping;
                    transform.rotation = Quaternion.AngleAxis(90 + angle, Vector3.forward);
                    _moveDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                    _lookAtDirection = _moveDirection;
                }
            }
            else if (Input.GetKeyDown(KeyCode.S))// && currentGridPos.y > 0)
            {
                currentGridPos.y--;
            }
            else if (Input.GetKeyDown(KeyCode.D))// && currentGridPos.x < gridSize.x - 1)
            {
                currentGridPos.x++;
            }
            else if (Input.GetKeyDown(KeyCode.A))// && currentGridPos.x > 0)
            {
                currentGridPos.x--;
            }

            // Update the position of the object in world space
            transform.position = tilemap.GetCellCenterWorld(currentGridPos);
        }
    }
    
    public void BackToStartPos()
    {
        _t.position = _startPosition;
    }

    private IEnumerator ThrowMolotov()
    {
        var molotov = GameManager.Instance.MolotovPool.Get();
        // if(invisible)
        // molotov.SetVisibility(_spriteRenderer.enabled);
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
            building.SetStatus(GameManager.BURNING);
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
    
    
    
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name.StartsWith("Splash") || col.gameObject.name.StartsWith("FirePlace"))
        {
            var curPos = _t.position;
            // var splashPos = col.transform.position;
            if (curPos.x - 10 > -40)
                _t.position += 10 * Vector3.left;
            else
                _t.position += (-40 - curPos.x) * Vector3.right;
            Destroy(col.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        print(col.collider);
        if (col.collider.name.StartsWith("Water") && invisible)
        {
            _spriteRenderer.enabled = true;
            _shown = true;
        }
    }
}