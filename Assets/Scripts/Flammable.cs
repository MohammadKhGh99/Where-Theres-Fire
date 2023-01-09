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

    
    // current status
    public enum Status { NotOnFire, OnFire, FinishedBurning }

    public Status CurrentStatus { get; set; }


    // onFire variables
    private float _timeUntilBurnOut;

    // NotOnFire variables
    [SerializeField] private float increaseChancePercentage = 5f;
    private float _currentChanceOfInflammation;

    // objects around us and radius to capture them
    [SerializeField] private float ratioOfRadiusBySize = 4f;
    private HashSet<Flammable> _objectsAroundUs;
    private SortedList<float, Flammable> _objectsAroundUsSorted;
    private bool _inOrder;
    private bool _notFoundSomethingToBurn;
    
    
    
    // burning objects around us
    [SerializeField] private float cooldownToBurn = 3f;
    private float _passedTimeForCooldown;
    

    private void Start()
    {
        _t = GetComponent<Transform>();
        _boxCollider = GetComponent<BoxCollider2D>();
        
        _timeUntilBurnOut = initialTimeUntilBurnOut;
        _currentChanceOfInflammation = initialChanceOfInflammation;

        CurrentStatus = Status.NotOnFire;
        
        //experimental testing
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
            }

            if (_passedTimeForCooldown >= cooldownToBurn)
            {
                // we can now try to burn something around us
                if (_inOrder)
                {
                    int i = 0;
                    _notFoundSomethingToBurn = true;
                    while (i < _objectsAroundUsSorted.Count)
                    {
                        var (distance, flameScript) = _objectsAroundUsSorted.ElementAt(i);
                        if (flameScript.CurrentStatus.Equals(Status.NotOnFire))
                        {
                            // we want this!, else move to another one..
                            _notFoundSomethingToBurn = false;

                        }
                        
                        
                        i++;
                    }

                    if (_notFoundSomethingToBurn)
                    {
                        
                    }

                }
                else
                {
                    
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
            SetObjectOnFire();
            return true;
        }
        _currentChanceOfInflammation = Mathf.Min(increaseChancePercentage + _currentChanceOfInflammation, 100);
        return false;
    }
    
    
    private void SetObjectOnFire()
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
        var direction = area.x >= area.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        var collider2Ds = Physics2D.OverlapCapsuleAll(_t.position, area, direction, 0);
        
        // filter the colliders
        foreach (var col in collider2Ds)
        {
            // Flammable res;
            if (!col.gameObject.TryGetComponent(out Flammable res)) continue;
            _objectsAroundUs.Add(res);
                
            // sort by distance from pivot (MAYBE DELETE IT LATER)
            Vector2 position = _t.position;
            _objectsAroundUsSorted.Add(Vector2.Distance(position, Physics2D.ClosestPoint(position, col)), res);
        }
    }
}
