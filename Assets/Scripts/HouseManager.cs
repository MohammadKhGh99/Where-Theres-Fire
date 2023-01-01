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
    [SerializeField] private Slider healthBarObj;

    private float _maxBurningTime;
    private float _timeToBurn;
    private SpriteRenderer _spriteRenderer;
    private Transform _t;
    private BoxCollider2D _collider;

    // variables for ovelaping box around the house
    private Vector2 _overlapSize;
    private Collider2D[] _overlappingResults;
    private bool _madeInfection;
    private HouseManager[] _housesSurrounding;
    private bool _setOrNot;


    // Start is called before the first frame update
    void Start()
    {
        _t = transform;
        _maxBurningTime = healthBarLife;
        _timeToBurn = _maxBurningTime;
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // controlling health bar for this house
        healthBarObj.maxValue = _maxBurningTime;
        healthBarObj.value = _maxBurningTime;
        
        // saving all surrounding houses to an array
        _overlappingResults = new Collider2D[8];
        _housesSurrounding = new HouseManager[8];
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.IsGameRunning || status == GameManager.BURNED)
            return;
        if (!_setOrNot)
            SetSurroundings();
        
        switch (status)
        {
            case GameManager.BURNING when _timeToBurn > 0:
            {
                if (_maxBurningTime - _timeToBurn >= 2 && !_madeInfection)
                {
                    foreach (var building in _housesSurrounding)
                    {
                        if (building.IsUnityNull())
                            continue;
                        building.SetStatus(GameManager.BURNING);
                    }

                    _madeInfection = true;
                }

                healthBarObj.value -= Time.deltaTime;
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black,
                    Time.deltaTime / _timeToBurn);
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
            case GameManager.WATERING when _timeToBurn < _maxBurningTime:
            {
                _timeToBurn += Time.deltaTime;
                if (_timeToBurn >= _maxBurningTime)
                    SetStatus(GameManager.NORMAL);
                break;
            }
            case GameManager.WATERING:
                _timeToBurn = _maxBurningTime;
                break;
        }
    }

    private void SetSurroundings()
    {
        _setOrNot = true;
        
        Physics2D.OverlapBox( _collider.bounds.center,  _collider.size + Vector2.one,
            0, new ContactFilter2D { layerMask = GameManager.Instance.HousesMask }, _overlappingResults);
        
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


    public void SetStatus(string newStatus)
    {
        status = newStatus;
    }
}