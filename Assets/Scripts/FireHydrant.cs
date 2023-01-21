using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FireHydrant : MonoBehaviour
{
    [SerializeField] private float circleRadius;
    private Flammable _flamable;
    private Transform _t;

    private HashSet<Flammable> _objectsAroundUs;

    [SerializeField] private GameObject sprayHolder;
    [SerializeField] private float rotationSpeed;
    private Transform _tSprayHolder;
    private bool _sprinklersStatus;
    
    void Start()
    {
        _flamable = GetComponent<Flammable>();
        _t = GetComponent<Transform>();
        _sprinklersStatus = false;
        _objectsAroundUs = new HashSet<Flammable>();
        GetFlammableObjectsAroundUs(circleRadius);
        
        _tSprayHolder = sprayHolder.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_flamable.CurrentStatus == Flammable.Status.OnFire)
        {
            // the fire hydrant onfire, turn it on
            StartCoroutine(SprayWater());
            _flamable.CurrentStatus = Flammable.Status.FinishedBurning;
        }

        if (_sprinklersStatus)
        {
            // todo turn on sprays here
            _tSprayHolder.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator SprayWater()
    {
        _sprinklersStatus = true;
        yield return new WaitForSeconds(1f);

        FillAreaWithWaterTiles();
        ExtinguishFire(true);

        yield return new WaitForSeconds(7f);        // time that the water hydrant is on

        TurnOffSprinkles();
        ExtinguishFire(false);
    }
    
    private void TurnOffSprinkles()
    {
        // todo turn on sprays here
        _sprinklersStatus = false;
    }
    private void ExtinguishFire(bool wateringMode)
    {
        foreach (var flammableObj in _objectsAroundUs)
        {
            flammableObj.SetSelfWatering(wateringMode);
        }
    }


    private void FillAreaWithWaterTiles()
    {
        Vector2 center = _t.position; // center position of the circle
        float radius = circleRadius; // radius of the circle
        Vector2 min = center - Vector2.one * radius;
        Vector2 max = center + Vector2.one * radius;
        for (int x = (int)min.x; x <= (int)max.x; x++)
        {
            for (int y = (int)min.y; y <= (int)max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if ( Vector2.Distance(center, (Vector3) pos) <= radius)
                {
                    GameManager.SetTile(pos, GameManager.Instance.WaterFireTilemap, GameManager.Instance.WaterTile);
                }
            }
        }
    }
    
    
    
    
    
    private void GetFlammableObjectsAroundUs(float radius = 3.25f)
    {
        var collider2Ds = Physics2D.OverlapCircleAll(_t.position, radius);

        // filter the colliders
        foreach (var col in collider2Ds)
        {
            // Flammable res;
            if (!col.gameObject.TryGetComponent(out Flammable res)) continue;
            if (res.Equals(_flamable)) continue;

            _objectsAroundUs.Add(res);
        }
    }
}
