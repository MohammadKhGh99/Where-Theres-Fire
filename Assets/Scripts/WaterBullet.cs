using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterBullet : MonoBehaviour
{
    // components
    private Transform _t;
    private BoxCollider2D _boxCollider;
    
    // shooting force
    [SerializeField] private float waterBulletPower = 100f;
    
    // speed of stream
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float bulletDistance = 15f;

    // size of stream variables
    [SerializeField] private float bulletLifeTime = 3f;
    private const float InitialSizeX = 0.01f;
    private float _finalSizeX;
    private float _currentSizeX;
    
    // information from player
    private Vector3 _previousPosition;          // the frame before this of the player position
    private float _previousAngel;          // the frame before this of the player position
    private Vector3 _previousStartPosition;     // the frame before this of the position of where the bullet should start
    private Vector3 _direction;     // the frame before this of the position of where the bullet should start

    // shooting bullet and shrinking
    private Vector3 _diePosition;

    // initializing and Statuses
    private bool _hasInitialized = false;
    private bool _hasPositioned = false;
    public GameManager.WaterBulletStatus currentStatus;


    //dropping water on tiles
    static float timer = 0.0f;
    private float _checkingTileRatio = 1f;  // this is how many times we check and change the tile to tile of water, because calling the function toomuch isn't good
    
    // Start is called before the first frame update
    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = GetComponent<Transform>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _hasInitialized = true;
        }
        gameObject.SetActive(true);
        _currentSizeX = InitialSizeX;
        _finalSizeX = bulletDistance;
        currentStatus = GameManager.WaterBulletStatus.Start;
    }

    private Vector3 GetScaleFromSizeX(float xSize)
    {
        var tempScale = Vector3.one;
        tempScale.x = xSize;
        return tempScale;
    }

    public void EnlargeBullet(Vector3 playerCurrentPos, Vector3 playerLookAtDirection, Vector3 startPosition)
    {
        _direction = playerLookAtDirection;
        _previousStartPosition = startPosition;
        
        // rotate
        var angle = Vector2.SignedAngle(playerLookAtDirection, Vector2.right);
        if (!_previousAngel.Equals(angle))
            _t.RotateAround(_previousPosition, Vector3.forward, angle - _previousAngel);
        _previousAngel = angle;


        // reposition
        if (!_hasPositioned)
        {
            _t.position = startPosition;
            _hasPositioned = true;
        }
        else
        {
            _t.position += playerCurrentPos - _previousPosition;
        }
        _previousPosition = playerCurrentPos;

        // enlarge
        currentStatus = GameManager.WaterBulletStatus.Enlarge;
    }
    
    public void ShootBullet()
    {
        // todo this is a temporary solution for that small bullet:
        // TEMPORARY SOLUTION - IDK IF IT WORKS
        //*****
        if (_currentSizeX <= 0.05f)
        {
            GameManager.Instance.WaterBulletPool.Release(this);
        }
        //******
        
        
        var distance = _finalSizeX - _currentSizeX / 2;
        if (distance > 0)
        {
            _diePosition = distance * _direction + _previousStartPosition;
            currentStatus = GameManager.WaterBulletStatus.Shoot;
        }
        else
        {
            currentStatus = GameManager.WaterBulletStatus.Decrease;
        }
    }
    void Update()
    {
        if (currentStatus.Equals(GameManager.WaterBulletStatus.Enlarge))
        {
            // var bulletDirection = new Vector3(Mathf.Cos(_previousAngel * Mathf.Deg2Rad), Mathf.Sin(_previousAngel * Mathf.Deg2Rad), 0);
            var bulletDirection = _direction;
            
            RaycastHit2D hit = Physics2D.Raycast(_previousStartPosition, bulletDirection, _currentSizeX, GameManager.Instance.HousesMask);
            if (!hit.collider.IsUnityNull())
            {
                // we collider something, make sure if it's building 
                if(hit.collider.transform.gameObject.name.StartsWith("House"))
                {
                    // it's a building, so our target shouldn't cross it, should stay at this size! or shrink a bit
                    if (hit.distance < _currentSizeX)
                    {
                        // we need to shrink it to be as distance 
                        _currentSizeX = hit.distance;
                        _t.position = bulletDirection * (_currentSizeX / 2 + InitialSizeX / 2) + _previousStartPosition;
                        _t.localScale = GetScaleFromSizeX(_currentSizeX);
                        return;
                    }
                }
            }
            
            if (_currentSizeX >= _finalSizeX)
            {
                // we Reached Full size! add water to the tile there
                var waterDropPos = bulletDirection * (_currentSizeX / 2 + InitialSizeX / 2) + _previousStartPosition;
                AddWaterToTile(waterDropPos);
                // return;
            }
            
            // no collisions, nothing to impact, so just enlarge it
            _currentSizeX = Mathf.MoveTowards(_currentSizeX, _finalSizeX, Time.deltaTime * bulletSpeed);
            _t.position = bulletDirection * (_currentSizeX / 2 + InitialSizeX / 2) + _previousStartPosition;
            _t.localScale = GetScaleFromSizeX(_currentSizeX);
        }
        else if (currentStatus.Equals(GameManager.WaterBulletStatus.Shoot))
        {
            var target = _diePosition;
            var tempStartPoint = _t.position - _direction * _currentSizeX / 2;  
            RaycastHit2D hit = Physics2D.Raycast(tempStartPoint, _direction, _currentSizeX, GameManager.Instance.HousesMask);
            if (!hit.collider.IsUnityNull())
            {
                // we collider something, make sure if it's building 
                if(hit.collider.transform.gameObject.name.StartsWith("House"))
                {
                    // it's a building, so our target shouldn't cross it, should stay at this size! or shrink a bit
                    if (hit.distance < _currentSizeX)
                    {
                        // we need to Stop it, we reached building
                        currentStatus = GameManager.WaterBulletStatus.Decrease;
                        return;
                    }
                }
            }
            
            var currentPos = _t.position;
            var distance = (currentPos - target).magnitude;
            if (distance > 0.01f)
            {
                // Calculate the interpolation point
                var t = Mathf.Clamp(bulletSpeed * Time.deltaTime / distance, 0, 1);

                // Interpolate towards the target position
                _t.position = Vector3.Lerp(currentPos, target, t);
            }
            else
            {
                // we reached our location, now it's time to Decrease!
                _t.position = target;
                currentStatus = GameManager.WaterBulletStatus.Decrease;
            }
        }
        else if (currentStatus.Equals(GameManager.WaterBulletStatus.Decrease))
        {
            var tempEndPosition = _t.position + _direction * _currentSizeX / 2;  
            _currentSizeX = Mathf.MoveTowards(_currentSizeX, InitialSizeX, Time.deltaTime * bulletSpeed);
            _t.position = tempEndPosition - _direction * (_currentSizeX / 2);
            _t.localScale = GetScaleFromSizeX(_currentSizeX);
            if (_currentSizeX <= InitialSizeX)
            {
                // we shrank down, now it's time to disappear!
                currentStatus = GameManager.WaterBulletStatus.Done;
                GameManager.Instance.WaterBulletPool.Release(this);
            }
        }
    }

    public void FakeRelease()
    {
        // reset flags!
        _hasPositioned = false;
        gameObject.SetActive(false);
    }
    
    private void AddWaterToTile(Vector3 waterDropPos)
    {
        // Timer to track the elapsed time

        // Update the timer
        timer += Time.deltaTime;
        // Check if the timer has reached the "_checkingTileRatio"
        if (timer >= _checkingTileRatio)
        {
            // Reset the timer
            timer = 0.0f;

            var gridPosition = GameManager.GroundBaseTilemap.WorldToCell(waterDropPos);
            // todo maybe check if null?
            if (!GameManager.GroundBaseTilemap.GetTile(gridPosition).name.Equals(GameManager.WaterTile.name))
            {
                // this tile doesn't have water on it, so add it!
                GameManager.SetTileAndUpdateNeighbors(waterDropPos, GameManager.GroundBaseTilemap,
                    GameManager.WaterTile);
            }
        }
    }
    
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        // print("there is trigger");
        if (col.CompareTag("House"))
        {
            // print("*HOUSE* start watering");
            // we want it to water
            // watering
            col.GetComponent<HouseManager>().SetStatus(GameManager.WATERING);
        }
        else if (col.CompareTag("FireMolotov"))
        {
            col.GetComponent<FireMolotov>().Extinguish();
        }
        else if (!col.attachedRigidbody.IsUnityNull())
        {
            col.attachedRigidbody.AddForce(_direction * waterBulletPower * bulletSpeed, ForceMode2D.Force);
        }
    }
    
    private void OnTriggerStay2D(Collider2D col)
    {
        // print("there is trigger");
        if (col.CompareTag("House"))
        {
            // print("*HOUSE* is watering");
            // we want it to water
            // watering
            var cur = col.GetComponent<HouseManager>();
            if (!cur.GetStatus().Equals(GameManager.WATERING))
                cur.SetStatus(GameManager.WATERING);

        }
        else if (col.CompareTag("FireMolotov"))
        {
            col.GetComponent<FireMolotov>().Extinguish();
        }
        else if (!col.attachedRigidbody.IsUnityNull())
        {
            col.attachedRigidbody.AddForce(_direction * waterBulletPower * bulletSpeed, ForceMode2D.Force);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("House"))
        {
            // print("*HOUSE* stops watering");
            other.GetComponent<HouseManager>().SetStatus(GameManager.NORMAL);
        }
    }
}
