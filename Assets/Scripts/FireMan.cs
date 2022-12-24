using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class FireMan : MonoBehaviour
{
    // Gamemanager
    private Transform _t;
    private Rigidbody2D _rb;

    // movement
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool fourDirection;
    [SerializeField] private float distanceToBurnBuilding;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;

    // shooting fire
    private float _fireKeyHoldingTime = 0f;
    private bool _fireKeyDown = false;
    private float _cooldownToMolotov = 0f;
    private bool _burningBuildingAnimationStarted = false;

    // hiding ability 
    [SerializeField] private bool invisible;
    private Sprite _mySprite;
    private SpriteShapeRenderer _spriteRenderer;
    private bool _shown;
    private float _hideTime;

    // controls changing
    private const KeyCode Fire = KeyCode.T;
    private const KeyCode Right = KeyCode.D,
                          Left = KeyCode.A,
                          Up = KeyCode.W,
                          Down = KeyCode.S;

    private RaycastHit2D hit;
    private LayerMask _buildingsMask;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _lookAtDirection = Vector2.right;
        _buildingsMask =  LayerMask.GetMask("Building");
        hit = Physics2D.Raycast(_t.position, _lookAtDirection, distanceToBurnBuilding, layerMask: _buildingsMask);
        
        _spriteRenderer = GetComponent<SpriteShapeRenderer>();
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
        // *** Movement ***
        var xDirection = Input.GetAxis("Horizontal2");
        var yDirection = Input.GetAxis("Vertical2");
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            _t.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _lookAtDirection = _moveDirection;
            hit = Physics2D.Raycast(_t.position, _lookAtDirection, distanceToBurnBuilding, layerMask: _buildingsMask);
            print(hit.collider);
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
                Debug.Log("Molotov");
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
                Debug.Log("FIIRREEE BRRRRR");
                _fireKeyDown = false;
            }
        }
        _cooldownToMolotov = Mathf.Max(_cooldownToMolotov - Time.deltaTime, 0f);
        
        // *** Power-Up: hide the character ***
        if (_shown)
        {
            _hideTime -= Time.deltaTime;
            if (_hideTime <= 0)
            {
                _hideTime = 4.0f;
                _spriteRenderer.enabled = false;
                // _spriteRenderer.sprite = null;
                _shown = false;
            }
        }
    }

    private IEnumerator ThrowMolotov()
    {
        var molotov = GameManager.instance.MolotovPool.Get();
        yield return molotov.Shoot(_t.position, _lookAtDirection);
        // when we finish with the bomb, 
        var molotovDropPos = molotov.GetMolotovDropPos();

        

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
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
        // print(col.collider);
        if (col.collider.name.StartsWith("Water") && invisible)
        {
            _spriteRenderer.enabled = true;
            _shown = true;
        }
    }
}