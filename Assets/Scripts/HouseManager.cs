using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HouseManager : MonoBehaviour
{
    [SerializeField] private string status = "Normal";
    [SerializeField] private GameManager.HouseStatus currentStatus = GameManager.HouseStatus.Normal;
    [SerializeField] private float healthBarLife = 10.0f;
    [SerializeField] private float wateringTime = 3.0f;
    [SerializeField] private float wateringSpeed = 3;

    [FormerlySerializedAs("pointsOnBurning")] [SerializeField] private float pointsOnBurn = 1;
    // [SerializeField] private Slider healthBarObj;
    // [SerializeField] private Slider wateringBarObj;

    private float _maxBurningTime;
    private float _maxWateringTime;
    private float _timeToBurn;
    private float _timeToWater;
    private float _percentToInfect = 0.5f;
    private SpriteRenderer _spriteRenderer;
    private Transform _t;
    private BoxCollider2D _collider;

    private GameObject _healthBar = null;
    private Image _healthBarImage;
    private Slider _healthBarObj;

    // variables for overlapping box around the house
    private Vector2 _overlapSize;
    private Collider2D[] _overlappingResults;
    private bool _madeInfection;
    private HouseManager[] _housesSurrounding;
    private bool _setOrNot;
    private Color _initialColor;


    // Start is called before the first frame update
    void Start()
    {
        // ** Components **
        _t = transform;
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        
        // ** time to burn and to water **
        _maxBurningTime = healthBarLife;
        _maxWateringTime = wateringTime;
        _timeToBurn = _maxBurningTime;
        _timeToWater = _maxWateringTime;
        

        // controlling health bar for this house
        if (!_healthBar.IsUnityNull())
        {
            _healthBarObj.maxValue = _maxBurningTime;
            _healthBarObj.value = _maxBurningTime;
        }
        
        // controlling watering bar for this house
        // wateringBarObj.maxValue = _maxWateringTime;
        // wateringBarObj.value = 0;
        
        // saving all surrounding houses to an array
        _overlappingResults = new Collider2D[8];
        _housesSurrounding = new HouseManager[8];
        
        // initial color
        _initialColor = _spriteRenderer.material.color;

    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.IsGameRunning || currentStatus.Equals(GameManager.HouseStatus.Burned))
            return;
        if (!_setOrNot)
            SetSurroundings();
        
        if (_healthBar.IsUnityNull())
            return;
        
        switch (currentStatus)
        {
            case GameManager.HouseStatus.Burning when _timeToBurn > 0:
            {
                if ((_healthBarObj.maxValue - _healthBarObj.value) / _healthBarObj.maxValue >= _percentToInfect && !_madeInfection)
                {
                    var numOfNeighborToBurn = (int)Mathf.Ceil((float)_housesSurrounding.Length / 3); 
                    foreach (var building in _housesSurrounding)
                    {
                        if (!building.IsUnityNull() && building.GetStatus().Equals(GameManager.HouseStatus.Normal))
                        {
                            building.SetStatus(GameManager.HouseStatus.Burning);
                            numOfNeighborToBurn--;
                        }
                        if (numOfNeighborToBurn <= 0)
                            break;
                    }

                    _madeInfection = true;
                }

                _healthBarObj.value -= Time.deltaTime;
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black,
                    Time.deltaTime / _timeToBurn);
                _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.red, Time.deltaTime / _timeToBurn);
                _timeToBurn -= Time.deltaTime;
                if (_timeToBurn <= 0)
                {
                    GameManager.Instance.burnedHousesBar.value += pointsOnBurn;
                    SetStatus(GameManager.HouseStatus.Burned);
                    GameManager.NumBurnedHouses++;
                    _spriteRenderer.color = Color.black;
                }

                break;
            }
            case GameManager.HouseStatus.Watering when _healthBarObj.value > 0:
            {
                _timeToWater -= Time.deltaTime;
                _timeToBurn += Time.deltaTime;
                // wateringBarObj.value += Time.deltaTime;
                _healthBarObj.value += Time.deltaTime * wateringSpeed;
                
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, _initialColor,
                    wateringSpeed * Time.deltaTime / _timeToWater);
                
                _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.green, wateringSpeed * Time.deltaTime / _timeToWater);
                
                if(_healthBarObj.value >= _healthBarObj.maxValue)
                {
                    SetStatus(GameManager.HouseStatus.Normal);
                    _timeToBurn = _maxBurningTime;
                    _timeToWater = _maxWateringTime;
                }
                // if (_timeToWater <= 0)
                // {
                //     SetStatus(GameManager.HouseStatus.Normal);
                //     _timeToWater = _maxWateringTime;
                // }
                break;
            }
            case GameManager.HouseStatus.Normal when _healthBarObj.value < _healthBarObj.maxValue: // when _timeToWater > 0 && _timeToWater < _maxWateringTime:
            {
                SetStatus(GameManager.HouseStatus.Burning);
                break;
            }
        }
    }

    public void SetHealthBar(GameObject other)
    {
        _healthBar = other;
        _healthBarObj = _healthBar.GetComponent<Slider>();
        _healthBarObj.maxValue = _maxBurningTime;
        _healthBarObj.value = _maxBurningTime;
        _healthBarImage = _healthBarObj.transform.GetChild(0).GetComponent<Image>();
    }

    private void SetSurroundings()
    {
        _setOrNot = true;
        
        Physics2D.OverlapBox( _collider.bounds.center,  _collider.size + Vector2.one,
            0, new ContactFilter2D { layerMask = Singleton<GameManager>.Instance.HousesMask }, _overlappingResults);
        
        if (_overlappingResults.Length == 0) return;
        
        for (int i = 0; i < _overlappingResults.Length; i++)
        {
            if (_overlappingResults[i].IsUnityNull()) continue;
            
            if (_overlappingResults[i].name.Equals(_collider.name))
            {
                _housesSurrounding[i] = null;
                continue;
            }
                
            // it is not expensive! it called once for each house, see _setOrNot variable...
            _housesSurrounding[i] = _overlappingResults[i].GetComponent<HouseManager>();
        }
    }
    
    public Vector2 GetBuildingPos()
    {
        return _t.position;
    }

    public GameManager.HouseStatus GetStatus()
    {
        return currentStatus;
    }

    public void SetStatus(GameManager.HouseStatus newStatus)
    {
        currentStatus = newStatus;
    }
    
    // public void StartGame()
    // {
    //     _spriteRenderer.material.color = _initialColor;
    //     _healthBarImage.color = Color.green;
    //     healthBarObj.value = healthBarObj.maxValue;
    //     currentStatus = GameManager.HouseStatus.Normal;
    //     _timeToBurn = _maxBurningTime;
    //     _timeToWater = _maxWateringTime;
    // }
}