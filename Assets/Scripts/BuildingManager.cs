using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private string status = "Normal";
    [SerializeField] private float healthBar = 5.0f;
    [SerializeField] private Slider healthBarObj;
    

    private const float MaxBurningTime = 5.0f;
    private float _timeToBurn = MaxBurningTime;
    private SpriteRenderer _spriteRenderer;
    private Color _initialColor;
    
    // Start is called before the first frame update
    void Start()
    {
        _timeToBurn = MaxBurningTime;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
        
        // healthBarObj = GetComponent<Slider>();
        healthBarObj.maxValue = MaxBurningTime;
        healthBarObj.value = MaxBurningTime;
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            case GameManager.BURNING when _timeToBurn > 0:
            {
                print(status);
                // healthBar -= Time.deltaTime;
                healthBarObj.value -= Time.deltaTime;
                print(healthBar);
                // print(status);
                // StartCoroutine(ChangeEngineColour());
                _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black, Time.deltaTime / _timeToBurn);
                // print(_initialColor);
                // _spriteRenderer.color = Color.Lerp(_initialColor, Color.black, 5);
                _timeToBurn -= Time.deltaTime;
                if (_timeToBurn <= 0)
                {
                    SetStatus(GameManager.BURNED);
                    print(status);
                    _spriteRenderer.color = Color.black;
                    // _timeToBurn = MaxBurningTime;
                    
                }

                break;
            }
            // print(status);
            case GameManager.WATERING when _timeToBurn < MaxBurningTime:
            {
                _timeToBurn += Time.deltaTime;
                if (_timeToBurn >= MaxBurningTime)
                    SetStatus(GameManager.NORMAL);
                break;
            }
            case GameManager.WATERING:
                _timeToBurn = MaxBurningTime;
                break;
        }
    }
    
    private IEnumerator ChangeEngineColour()
    {
        float tick = 0f;
        while (_spriteRenderer.material.color != Color.black)
        {
            tick += Time.deltaTime * 2;
            _spriteRenderer.material.color = Color.Lerp(_spriteRenderer.material.color, Color.black, Time.deltaTime / _timeToBurn);
            yield return null;
        }
    }

    public void SetStatus(string newStatus)
    {
        status = newStatus;
    }

    // private void OnCollisionEnter2D(Collision2D col)
    // {
    //     print(col.collider);
    //     if (col.collider.name.StartsWith("Water"))
    //     {
    //         SetStatus(GameManager.BURNING);
    //     }
    //     // if (col.collider.name.StartsWith("Water"))
    //     // {
    //     //     SetStatus(GameManager.WATERING);
    //     // }
    // }
 
}
