using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaterMan : MonoBehaviour
{
    // components
    private Transform _t;
    private Rigidbody2D _rb;
    
    // movement
    [SerializeField] private float movingSpeed;
    [SerializeField] private bool fourDirection;
    [SerializeField] private float distanceToBurnBuilding = 5;
    [SerializeField] private Slider waterCoolDownSlider;
    private Vector2 _moveDirection;
    private Vector2 _lookAtDirection;
    
    // shooting fire
    private float _fireKeyHoldingTime = 0f;
    private bool _fireKeyDown = false;
    private float _cooldownToWaterGun = 0f;
    private bool _burningBuildingAnimationStarted = false;
    
    // controls changing
    private const KeyCode Fire = KeyCode.Period;
    private const KeyCode Right = KeyCode.RightArrow,
                          Left = KeyCode.LeftArrow,
                          Up = KeyCode.UpArrow,
                          Down = KeyCode.DownArrow;

    private ParticleSystem _waterSplash;
    private RaycastHit2D _hit;
    private LayerMask _buildingsMask;
    private SplashBullet _splashBullet;
    private Image _waterCoolSlideImage;
    private bool _canWater = true;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _t = GetComponent<Transform>();
        _lookAtDirection = Vector2.left;
        
        _waterSplash = _t.GetChild(0).GetComponent<ParticleSystem>();
        _splashBullet = _t.GetChild(1).GetComponent<SplashBullet>();
        _splashBullet.FakeStart();
        _splashBullet.SetWatering(true);
        _buildingsMask =  LayerMask.GetMask("Building");
        
        // todo - solve the problem that the 3 seconds here is almost the same as 5 seconds in buildings!!!
        waterCoolDownSlider.maxValue = 3.0f;
        waterCoolDownSlider.value = 0;
        _waterCoolSlideImage = waterCoolDownSlider.fillRect.GetComponent<Image>();
        
        _hit = Physics2D.Raycast(_t.position, _lookAtDirection, distanceToBurnBuilding, layerMask: _buildingsMask);
    }

    private void Update()
    {
        // *** Movement ***
        var xDirection = Input.GetAxis("Horizontal1");
        var yDirection = Input.GetAxis("Vertical1");
        _moveDirection.x = xDirection;
        _moveDirection.y = yDirection;
        _hit = Physics2D.Raycast(_t.position, _lookAtDirection, distanceToBurnBuilding, layerMask: _buildingsMask);
        
        if ((!_fireKeyDown || _hit.collider.IsUnityNull()) || (!_canWater && waterCoolDownSlider.value > 0))
            waterCoolDownSlider.value -= Time.deltaTime;
        
        var snapping = fourDirection ? 90.0f : 45.0f;
        if (_moveDirection.sqrMagnitude > 0)
        {
            var angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / snapping) * snapping;
            _t.rotation = Quaternion.AngleAxis( 90 + angle, Vector3.forward);
            _moveDirection = Quaternion.AngleAxis( angle, Vector3.forward) * Vector3.right;
            _lookAtDirection = _moveDirection;
            
            // print(hit.collider);
        }
        
        // *** shooting ability ***
        if (Input.GetKeyDown(Fire))
        {
            _fireKeyDown = true;
        }
        else if (Input.GetKeyUp(Fire))
        {
            if (_fireKeyHoldingTime < 0.2f && _cooldownToWaterGun <= 0f)
            {
                // throwing a short water splash in the direction the player is looking at. (unless its a building then nothing)
                // print("Water Splash");
                _cooldownToWaterGun = GameManager.SplashBulletCooldownTime;
                
                // _splashBullet.SetWatering(false);
                StartCoroutine(ThrowSplashBullet());

            }

            StopWatering();
            // _splashBullet.gameObject.SetActive(false);
            _fireKeyHoldingTime = 0f;
            _fireKeyDown = false;
            _burningBuildingAnimationStarted = false;
        }

        if (_hit.collider.IsUnityNull())
            StopWatering();

        
        // print(_wateringCoolDown + " " + _fireKeyDown);
        
        // print(hit.collider);
        if (_fireKeyDown && _canWater && !_hit.collider.IsUnityNull())
        {
            if (_burningBuildingAnimationStarted && waterCoolDownSlider.value < 3.0f)
                waterCoolDownSlider.value += Time.deltaTime;
            // _wateringCoolDown -= Time.deltaTime;
            _fireKeyHoldingTime += Time.deltaTime;
            if (_fireKeyHoldingTime >= 0.5f && !_burningBuildingAnimationStarted)
            {
                // print("Wateriiiing");
                // start watering building animation 
                var main = _waterSplash.main;
                main.duration = 10;
                main.loop = true;
                main.startLifetime = 1;
                var shape = _waterSplash.shape;
                shape.randomDirectionAmount = 0.45f;
                _waterSplash.Play();
                _splashBullet.gameObject.SetActive(true);
                _burningBuildingAnimationStarted = true;
            }
            if (_fireKeyHoldingTime >= 5f)
            {
                // the torch is thrown in the -building- and it will start to burn - stop animation also
                // print("Water is everywhere! (5 sec)");
                StopWatering();
                
                StartCoroutine(WateringCoolDown());
                _fireKeyDown = false;
                _burningBuildingAnimationStarted = false;
            }
        }
        _cooldownToWaterGun = Mathf.Max(_cooldownToWaterGun - Time.deltaTime, 0f);
        
    }

    private void StopWatering()
    {
        _waterSplash.Stop();
        _splashBullet.Die();
    }

    private IEnumerator WateringCoolDown()
    {
        _canWater = false;
        yield return new WaitForSeconds(3);
        _canWater = true;
    }
    
    private IEnumerator ThrowSplashBullet()
    {
        // print("splashing");
        var splash = GameManager.instance.SplashBulletPool.Get();
        // var main = _waterSplash.main;
        // main.duration = 1;
        // main.loop = false;
        // main.startLifetime = 10;
        // var shape = _waterSplash.shape;
        // shape.randomDirectionAmount = 0.1f;
        // _waterSplash.Play();
        yield return splash.Shoot(_t.position, _lookAtDirection);
        // when we finish with the bomb, 
        var molotovDropPos = _splashBullet.GetSplashBulletDropPos();

        

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * (movingSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name.StartsWith("FirePlace"))
        {
            var curPos = _t.position;
            if (curPos.x + 10 < 40)
                _t.position += 10 * Vector3.right;
            else
                _t.position += (40 - curPos.x) * Vector3.right;
            // Destroy(col.gameObject);
        }
    }
}