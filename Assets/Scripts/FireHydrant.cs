using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FireHydrant : MonoBehaviour
{
    [SerializeField] private float circleRadius;
    private Flammable _flammable;
    private Transform _t;

    private LayerMask fireMolotovMask;
    private HashSet<Flammable> _objectsAroundUs;

    [SerializeField] private GameObject sprayHolder;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float timeSprayIsOn = 7f;
    private Transform _tSprayHolder;
    private bool _sprinklersStatus;
    private ParticleSystem _firstSpray, _secondSpray;
    
    void Start()
    {
        _flammable = GetComponent<Flammable>();
        _t = GetComponent<Transform>();
        _sprinklersStatus = false;
        _objectsAroundUs = new HashSet<Flammable>();

        fireMolotovMask = LayerMask.GetMask("FireMolotov");
        GetFlammableObjectsAroundUs(circleRadius);
        
        _tSprayHolder = sprayHolder.GetComponent<Transform>();
        _firstSpray = _tSprayHolder.GetChild(0).GetComponent<ParticleSystem>();
        _secondSpray = _tSprayHolder.GetChild(1).GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_flammable.CurrentStatus == Flammable.Status.OnFire)
        {
            // the fire hydrant onfire, turn it on
            StartCoroutine(SprayWater());
            _flammable.CurrentStatus = Flammable.Status.FinishedBurning;
        }

        if (_sprinklersStatus)
        {
            if (_firstSpray.isStopped && _secondSpray.isStopped)
            {
                _firstSpray.Play();
                _secondSpray.Play();
            }
            _tSprayHolder.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator SprayWater()
    {
        _sprinklersStatus = true;
        yield return new WaitForSeconds(0.3f);
        
        // time that the water hydrant is on
        for(var i = 0; i < timeSprayIsOn; i++)
        {
            // if we find firemolotov, turn it off!
            var collider2Ds = Physics2D.OverlapCircleAll(_t.position, circleRadius, fireMolotovMask);
            foreach (var col in collider2Ds)
            {
                col.gameObject.GetComponent<FireMolotov>().Extinguish();
            }
            
            FillAreaWithWaterTiles(circleRadius + 0.25f*i);
            ExtinguishFire(true);
            yield return new WaitForSeconds(1f);
        } 

        TurnOffSprinkles();
        ExtinguishFire(false);
    }
    
    private void TurnOffSprinkles()
    {
        // todo turn off sprays here
        if (_firstSpray.isPlaying && _secondSpray.isPlaying)
        {
            _firstSpray.Stop();
            _secondSpray.Stop();
        }
        _sprinklersStatus = false;
    }
    private void ExtinguishFire(bool wateringMode)
    {
        foreach (var flammableObj in _objectsAroundUs)
        {
            flammableObj.SetSelfWatering(wateringMode);
        }
    }


    private void FillAreaWithWaterTiles(float radius)
    {
        Vector2 center = _t.position; // center position of the circle
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
            if (res.Equals(_flammable)) continue;

            _objectsAroundUs.Add(res);
        }
    }
}
