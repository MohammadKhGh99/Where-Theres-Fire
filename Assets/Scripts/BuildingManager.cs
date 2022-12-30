using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private string status = "Normal";
    [SerializeField] private float healthBarLife = 10.0f;
    [SerializeField] private Slider healthBarObj;
    // [SerializeField] private Color fullBarColor;
    // [SerializeField] private Color emptyBarColor;
    

    private float _maxBurningTime = 10.0f;
    private float _timeToBurn;
    private SpriteRenderer _spriteRenderer;
    private Color _initialColor;
    private Image _healthBarSlideImage;
    private Transform _t;

    // Start is called before the first frame update
    void Start()
    {
        _t = transform;
        _maxBurningTime = healthBarLife;
        _timeToBurn = _maxBurningTime;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
        
        // healthBarObj = GetComponent<Slider>();
        healthBarObj.maxValue = _maxBurningTime;
        healthBarObj.value = _maxBurningTime;
        _healthBarSlideImage = healthBarObj.fillRect.GetComponent<Image>();
        // _healthBarSlideImage.color = fullBarColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.IsGameRunning || status == GameManager.BURNED)
            return;
        switch (status)
        {
            case GameManager.BURNING when _timeToBurn > 0:
            {
                // print(status);
                // healthBar -= Time.deltaTime;
                healthBarObj.value -= Time.deltaTime;
                // Color color = _healthBarSlideImage.material.color; 
                // color.r = 1 - (_timeToBurn / MaxBurningTime);
                // color.g = _timeToBurn / MaxBurningTime;
                // _healthBarSlideImage.material.color = color;
                
                // _healthBarSlideImage.color = Color.Lerp(_healthBarSlideImage.color, fullBarColor, Time.deltaTime / _timeToBurn);
                // print(healthBar);
                // print(status);
                // StartCoroutine(ChangeEngineColour());
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black, Time.deltaTime / _timeToBurn);
                // print(_initialColor);
                // _spriteRenderer.color = Color.Lerp(_initialColor, Color.black, 5);
                _timeToBurn -= Time.deltaTime;
                if (_timeToBurn <= 0)
                {
                    SetStatus(GameManager.BURNED);
                    GameManager.NumBurnedBuildings++;
                    print(status);
                    _spriteRenderer.color = Color.black;
                    // _timeToBurn = MaxBurningTime;
                    
                }

                break;
            }
            // print(status);
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

    public Vector2 GetBuildingPos()
    {
        return _t.position;
    }
    

    public void SetStatus(string newStatus)
    {
        status = newStatus;
    }
    
 
}
