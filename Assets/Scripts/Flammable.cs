using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class Flammable : MonoBehaviour
{
    // points on item
    [SerializeField] private float numOfPoints;
    [SerializeField] private bool isHouse;

    // initializing variables
    [SerializeField] private bool isFireSource;
    [SerializeField] private float initialTimeUntilBurnOut;
    [SerializeField] private float initialChanceOfInflammation;
    private Color _initialColor;

    private Transform _t;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    // onFire and Extinguishing others variables
    private float _timeUntilBurnOut;
    [SerializeField] private float extinguishingSpeed;
    private bool _gettingExtinguished;


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
    
    // health bar
    private GameObject _healthBar;
    private Image _healthBarImage;
    private Slider _healthBarObj;
    private Color _healthBarColor;
    


    // current status
    public enum Status { NotOnFire, OnFire, FinishedBurning }

    public Status CurrentStatus { get; set; }
    
    private void Start()
    {
        _t = GetComponent<Transform>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
        _objectsAroundUs = new HashSet<Flammable>();
        _objectsAroundUsSorted = new SortedList<float, Flammable>();

        // initializing health bar
        if (!_healthBar.IsUnityNull())
        {
            _healthBarObj = _healthBar.GetComponent<Slider>();
            _healthBarObj.maxValue = initialTimeUntilBurnOut;
            _healthBarObj.value = initialTimeUntilBurnOut;
            _healthBarImage = _healthBarObj.transform.GetChild(0).GetComponent<Image>();
            _healthBarColor = _healthBarImage.color;
        }
        
        if (isFireSource)
        {
            _currentChanceOfInflammation = 0;
            _timeUntilBurnOut = Mathf.Infinity;
        }
        else
        {
            _currentChanceOfInflammation = initialChanceOfInflammation;
            _timeUntilBurnOut = initialTimeUntilBurnOut;
        }
        
        CurrentStatus = Status.NotOnFire;
        
        // we need to collect all the objects around us now, and we randomly burn an object from the list
        if (!isFireSource)
        {
            GetFlammableObjectsAroundUs();
        }

        // *** experimental testing ***
        _inOrder = true;
    }
    
    private Vector2 GetSizeOfArea()
    {
        var objectSize = _collider.bounds.size;
        objectSize += objectSize / ratioOfRadiusBySize;
        return objectSize;
    }
    
    private void Update()
    {
        if (!CurrentStatus.Equals(Status.OnFire)) return;
        
        _timeUntilBurnOut -= Time.deltaTime;
        if (isHouse)
            _healthBarObj.value -= Time.deltaTime;
        if (_timeUntilBurnOut <= 0f)
        {
            // the object is completely burnt!
            CurrentStatus = Status.FinishedBurning;
            BurnedOutEffectAndPoints();
            return;
        }

        ChangingSpriteColorBecauseOfFireOrWater();
        
        // burn something around you
        if (_passedTimeForCooldown >= cooldownToBurn)
        {
            _passedTimeForCooldown = 0f;
            int chanceFromBurnTime;
            if (isFireSource)
            {
                chanceFromBurnTime = 100;
            }
            else
            {
                chanceFromBurnTime = (int)((1 - _timeUntilBurnOut / initialTimeUntilBurnOut) * 100);
            }

            // we can now try to burn something around us
            if (_inOrder)
            {
                foreach (var (distance, otherFlameScript) in _objectsAroundUsSorted)    // todo make sure we go from negative (small) to positive (big)
                {
                    if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;
                        
                    // we want this, try to burn it!, else move to another one.. (distance < _maxDistanceFromOrigin)
                    var chanceFromDistance = (int)((1 - distance / _maxDistanceFromOrigin) * 100);

                    otherFlameScript.TryToBurn(chanceFromDistance, chanceFromBurnTime);     // true if it burned, false if not
                }
            }
            else
            {
                foreach (var otherFlameScript in _objectsAroundUs)
                {
                    if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;
                        
                    // we want this, try to burn it!, else move to another one..
                    otherFlameScript.TryToBurn(50, chanceFromBurnTime);     // true if it burned, false if not
                }
            }
        }
        _passedTimeForCooldown += Time.deltaTime;

        if (_gettingExtinguished)
        {
            GettingExtinguished();
        }
    }


    private void BurnedOutEffectAndPoints()
    {
        _spriteRenderer.color = Color.black;
        GameManager.Instance.burnedHousesBar.value += numOfPoints;
        if (isHouse)
        {
            GameManager.NumBurnedHouses++;
        }
    }

    private void ChangingSpriteColorBecauseOfFireOrWater()
    {
        _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, Color.black, Time.deltaTime / _timeUntilBurnOut);
        if (!isHouse) return;
        if (CurrentStatus.Equals(Status.OnFire))
            _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.red, Time.deltaTime / _timeUntilBurnOut);
        else
            _healthBarImage.color = Color.Lerp(_healthBarImage.color, _healthBarColor, Time.deltaTime / _timeUntilBurnOut);
        // _healthBarObj.value -= Time.deltaTime;
    }
    
    public void SetHealthBar(GameObject other)
    {
        _healthBar = other;
        _healthBarObj = _healthBar.GetComponent<Slider>();
        _healthBarObj.maxValue = initialTimeUntilBurnOut;
        _healthBarObj.value = initialTimeUntilBurnOut;
        _healthBarImage = _healthBarObj.transform.GetChild(0).GetComponent<Image>();
        _healthBarColor = _healthBarImage.color;
    }


    public bool TryToBurn(int chanceFromDistance, int chanceFromBurnTime)
    {
        // this is the main function that other people call when they try to burn object.
        // check if it has a chance to get burned using chance of distance from flame source, and material chance.
        // return true if it got burned, false if not.
        if (isFireSource)
            return false;

        var realChance = (chanceFromDistance * 2 + _currentChanceOfInflammation * 2 + chanceFromBurnTime) / 5.0f; 
        if (Random.Range(0, 100) <= realChance)
        {
            // this object will get burned, call function that will make it burn, 
            SetSelfOnFire();
            return true;
        }
        _currentChanceOfInflammation = Mathf.Min(increaseChancePercentage + _currentChanceOfInflammation, 100);
        return false;
    }
    
    
    public void SetSelfOnFire()
    {
        // now set it on fire 
        // todo things to make this object on fire, maybe call something from real code? idk
        if(isFireSource)
            GetFlammableObjectsAroundUs();
        CurrentStatus = Status.OnFire;
    }

    public void SetSelfWatering(bool status)
    {
        _gettingExtinguished = status;
    }
    
    private void GettingExtinguished()
    {
        _timeUntilBurnOut += Time.deltaTime * extinguishingSpeed;
        if (!_healthBar.IsUnityNull())
            _healthBarObj.value += Time.deltaTime * extinguishingSpeed;
        if (!(_timeUntilBurnOut >= initialTimeUntilBurnOut)) return;
        // we watered the object
        initialTimeUntilBurnOut = _timeUntilBurnOut;
        _spriteRenderer.color = _initialColor;
        CurrentStatus = Status.NotOnFire;
    }

    public Vector2 GetPosition()
    {
        return _t.position;
    }

    private void GetFlammableObjectsAroundUs()
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
            if(res.Equals(this)|| res.isFireSource) continue;
            
            _objectsAroundUs.Add(res);
                
            // sort by distance of colliders (MAYBE DELETE IT LATER)
            _objectsAroundUsSorted.Add(Physics2D.Distance(_collider, col).distance, res);

        }
    }
}
