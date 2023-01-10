using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flammable : MonoBehaviour
{
    // initializing variables
    [SerializeField] private float initialTimeUntilBurnOut;
    [SerializeField] private float initialChanceOfInflammation;

    private Transform _t;
    private BoxCollider2D _boxCollider;
    
    // onFire and burning others variables
    private float _timeUntilBurnOut;

    // NotOnFire variables
    [SerializeField] private float increaseChancePercentage = 5f;
    private float _currentChanceOfInflammation;

    // objects around us and radius to capture them
    [SerializeField] private float ratioOfRadiusBySize = 4f;
    private float _maxDistanceFromOrigin;
    private HashSet<Flammable> _objectsAroundUs;
    private SortedList<float, Flammable> _objectsAroundUsSorted;
    private bool _inOrder;
    private bool _notFoundSomethingToBurn;
    
    // burning objects around us
    [SerializeField] private float cooldownToBurn = 3f;
    private float _passedTimeForCooldown;
    

    // current status
    public enum Status { NotOnFire, OnFire, FinishedBurning }

    public Status CurrentStatus { get; set; }
    
    private void Start()
    {
        _t = GetComponent<Transform>();
        _boxCollider = GetComponent<BoxCollider2D>();
        
        _timeUntilBurnOut = initialTimeUntilBurnOut;
        _currentChanceOfInflammation = initialChanceOfInflammation;

        CurrentStatus = Status.NotOnFire;
        
        // *** experimental testing ***
        _inOrder = true;
    }
    
    private Vector2 GetSizeOfArea()
    {
        var objectSize = _boxCollider.size;
        objectSize += objectSize / ratioOfRadiusBySize;
        return objectSize;
    }
    
    private void Update()
    {
        if (CurrentStatus.Equals(Status.OnFire))
        {
            _timeUntilBurnOut += Time.deltaTime;
            if (_timeUntilBurnOut >= initialTimeUntilBurnOut)
            {
                // the object is completely burnt!
                CurrentStatus = Status.FinishedBurning;
                return;
            }

            if (_passedTimeForCooldown >= cooldownToBurn)
            {
                _passedTimeForCooldown = 0f;
                
                // we can now try to burn something around us
                if (_inOrder)
                {
                    foreach (var (distance, otherFlameScript) in _objectsAroundUsSorted)
                    {
                        if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;
                        
                        // we want this, try to burn it!, else move to another one..
                        var chanceFromDistance = (int)(distance / _maxDistanceFromOrigin * 100);
                        otherFlameScript.TryToBurn(chanceFromDistance);     // true if it burned, false if not
                    }
                }
                else
                {
                    foreach (var otherFlameScript in _objectsAroundUs)
                    {
                        if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;
                        
                        // we want this, try to burn it!, else move to another one..
                        otherFlameScript.TryToBurn(50f);     // true if it burned, false if not
                    }
                }
            }
            _passedTimeForCooldown += Time.deltaTime;
        }
    }


    public bool TryToBurn(float chanceFromDistance)
    {
        // this is the main function that other people call when they try to burn object.
        // check if it has a chance to get burned using chance of distance from flame source, and material chance.
        // return true if it got burned, false if not.
        var realChance = (chanceFromDistance + _currentChanceOfInflammation) / 2.0f;
        if (Random.Range(0, 100) <= realChance)
        {
            // this object will get burned, call function that will make it burn, 
            SetSelfOnFire();
            return true;
        }
        _currentChanceOfInflammation = Mathf.Min(increaseChancePercentage + _currentChanceOfInflammation, 100);
        return false;
    }
    
    
    private void SetSelfOnFire()
    {
        // i guess we need to collect all the objects around us now, and we randomly burn an object from the list
        GetFlammableObjectsAroundUs();
        // now set it on fire
        CurrentStatus = Status.OnFire;
    }

    private void GetFlammableObjectsAroundUs()
    // TODO, maybe you should run this at the start, it depends on the performance.
    {
        var area = GetSizeOfArea();
        _maxDistanceFromOrigin = Mathf.Max(area.x, area.y);
        var direction = area.x >= area.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        var collider2Ds = Physics2D.OverlapCapsuleAll(_t.position, area, direction, 0);
        
        // filter the colliders
        foreach (var col in collider2Ds)
        {
            // Flammable res;
            if (!col.gameObject.TryGetComponent(out Flammable res)) continue;
            _objectsAroundUs.Add(res);
                
            // sort by distance of colliders (MAYBE DELETE IT LATER)
            _objectsAroundUsSorted.Add(Physics2D.Distance(_boxCollider, col).distance, res);
        }
    }
}
