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
    [SerializeField] private float healthBarLife = 10.0f;
    [SerializeField] private float wateringTime = 3.0f;
    [SerializeField] private float wateringSpeed = 3;
    [SerializeField] private Slider healthBarObj;
    [SerializeField] private Slider wateringBarObj;

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
        _healthBarImage = healthBarObj.transform.GetChild(0).GetComponent<Image>();
        
        // ** time to burn and to water **
        _maxBurningTime = healthBarLife;
        _maxWateringTime = wateringTime;
        _timeToBurn = _maxBurningTime;
        _timeToWater = _maxWateringTime;
        

        // controlling health bar for this house
        if (!_healthBar.IsUnityNull())
        {
            healthBarObj.maxValue = _maxBurningTime;
            healthBarObj.value = _maxBurningTime;
        }
        
        // controlling watering bar for this house
        wateringBarObj.maxValue = _maxWateringTime;
        wateringBarObj.value = 0;
        
        // saving all surrounding houses to an array
        _overlappingResults = new Collider2D[8];
        _housesSurrounding = new HouseManager[8];
        
        // initial color
        _initialColor = _spriteRenderer.material.color;

    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.IsGameRunning || status == GameManager.BURNED)
            return;
        if (!_setOrNot)
            SetSurroundings();
        
        if (_healthBar.IsUnityNull())
            return;
        
        switch (status)
        {
            case GameManager.BURNING when _timeToBurn > 0:
            {
                if ((healthBarObj.maxValue - healthBarObj.value) / healthBarObj.maxValue >= _percentToInfect && !_madeInfection)
                {
                    var numOfNeighborToBurn = (int)Mathf.Ceil((float)_housesSurrounding.Length / 3); 
                    foreach (var building in _housesSurrounding)
                    {
                        if (!building.IsUnityNull() && building.GetStatus().Equals(GameManager.NORMAL))
                        {
                            building.SetStatus(GameManager.BURNING);
                            numOfNeighborToBurn--;
                        }
                        if (numOfNeighborToBurn <= 0)
                            break;
                    }

                    _madeInfection = true;
                }

                healthBarObj.value -= Time.deltaTime;
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black,
                    Time.deltaTime / _timeToBurn);
                _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.red, Time.deltaTime / _timeToBurn);
                _timeToBurn -= Time.deltaTime;
                if (_timeToBurn <= 0)
                {
                    SetStatus(GameManager.BURNED);
                    GameManager.NumBurnedHouses++;
                    print(status);
                    _spriteRenderer.color = Color.black;
                }

                break;
            }
            case GameManager.WATERING when healthBarObj.value > 0:
            {
                _timeToWater -= Time.deltaTime;
                _timeToBurn += Time.deltaTime;
                wateringBarObj.value += Time.deltaTime;
                healthBarObj.value += Time.deltaTime * wateringSpeed;
                
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, _initialColor,
                    wateringSpeed * Time.deltaTime / _timeToWater);
                
                _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.green, wateringSpeed * Time.deltaTime / _timeToWater);
                
                if(healthBarObj.value >= healthBarObj.maxValue)
                {
                    SetStatus(GameManager.NORMAL);
                    _timeToBurn = _maxBurningTime;
                    _timeToWater = _maxWateringTime;
                }
                // if (_timeToWater <= 0)
                // {
                //     SetStatus(GameManager.NORMAL);
                //     _timeToWater = _maxWateringTime;
                // }
                break;
            }
            case GameManager.NORMAL when healthBarObj.value < healthBarObj.maxValue: // when _timeToWater > 0 && _timeToWater < _maxWateringTime:
            {
                SetStatus(GameManager.BURNING);
                break;
            }
        }
    }

    public void SetHealthBar(GameObject other)
    {
        _healthBar = other;
        healthBarObj = _healthBar.GetComponent<Slider>();
        healthBarObj.maxValue = _maxBurningTime;
        healthBarObj.value = _maxBurningTime;
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

    public string GetStatus()
    {
        return status;
    }

    public void SetStatus(string newStatus)
    {
        status = newStatus;
    }
    
    // public void StartGame()
    // {
    //     _spriteRenderer.material.color = _initialColor;
    //     _healthBarImage.color = Color.green;
    //     healthBarObj.value = healthBarObj.maxValue;
    //     status = GameManager.NORMAL;
    //     _timeToBurn = _maxBurningTime;
    //     _timeToWater = _maxWateringTime;
    // }
}