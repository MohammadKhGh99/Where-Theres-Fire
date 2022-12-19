using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private string status = "Normal";

    private const string NORMAL = "Normal";
    private const string BURNING = "Burning";
    private const string BURNED = "Burned";
    private const string WATERING = "Watering";

    private const float MaxBurningTime = 5.0f;
    private float _timeToBurn = MaxBurningTime;
    private SpriteRenderer _spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        _timeToBurn = MaxBurningTime;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (status.Equals(BURNING))
        {
            print(status);
            _timeToBurn -= Time.deltaTime;
            if (_timeToBurn <= 0)
            {
                SetStatus(BURNED);
                _spriteRenderer.color = Color.black;
                _timeToBurn = MaxBurningTime;
            }
        } else if (status.Equals(WATERING))
        {
            print(WATERING);
            if (_timeToBurn < MaxBurningTime)
            {
                _timeToBurn += Time.deltaTime;
                if (_timeToBurn >= MaxBurningTime)
                    SetStatus(NORMAL);
            }else
                _timeToBurn = MaxBurningTime;
        }
    }

    public void SetStatus(string newStatus)
    {
        status = newStatus;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.name.StartsWith("Fire"))
        {
            SetStatus(BURNING);
        }
    }
}
