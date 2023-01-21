using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class Flammable : MonoBehaviour
{
    // points on item
    [SerializeField] private int numOfPoints;
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
    private float _currentHealth;
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
    private Image _healthBarImage;
    private Image _healthBarBorder;
    private Slider _healthBarObj;
    private bool _isImageVisible;

    // adding fire object to flammable image
    [SerializeField] private float timeBetweenAddingNewFire = 5f;
    private float _currentTimerToAddReleaseFire;
    private float _addFireTime;
    private float _releaseFireTime;
    private Bounds _bounds;
    private HashSet<FireObject> _firesOnImage = new();

    // ** points on burn out **
    private TextMeshProUGUI _points;
    // private float _pointsTravelY = 3;
    private float _targetPointsX = -20f;
    private float _targetPointsY = -6.5f;
    
    // *** Smoke ***
    private ParticleSystem _smoke;

    // current status
    public enum Status
    {
        NotOnFire,
        OnFire,
        FinishedBurning
    }

    public Status CurrentStatus { get; set; }

    private void Start()
    {
        _t = GetComponent<Transform>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
        _objectsAroundUs = new HashSet<Flammable>();
        _objectsAroundUsSorted = new SortedList<float, Flammable>();
        _bounds = _spriteRenderer.bounds;

        // adding fire object
        _addFireTime = 0f;
        _currentTimerToAddReleaseFire = 0f;
        _releaseFireTime = -timeBetweenAddingNewFire;

        if (isFireSource)
        {
            _currentChanceOfInflammation = 0;
            _timeUntilBurnOut = Mathf.Infinity;
            _currentHealth = Mathf.Infinity;
        }
        else
        {
            _bounds = _spriteRenderer.bounds;
            InitializeHealthBar();
            InitializePointsText();

            _currentChanceOfInflammation = initialChanceOfInflammation;
            _timeUntilBurnOut = initialTimeUntilBurnOut;
            _currentHealth = initialTimeUntilBurnOut;

            GetFlammableObjectsAroundUs();
            
            
            // *** Smoke Initializing ***
            
            var temp = Instantiate(Resources.Load("Smoke"), _t.position, Quaternion.identity, _t) as GameObject;
            if (temp == null) throw new NullReferenceException("There is no Smoke Prefabs!");
            _smoke = temp.GetComponent<ParticleSystem>();
            _smoke.transform.Rotate(Vector3.right, -90);
            if (!isHouse)
            {
                var newScale = new Vector3(0.5f, 1, 1);
                _smoke.transform.localScale = newScale;
                // var shapeModule = _smoke.shape;
                // shapeModule.angle = 0;
            }
        }

        CurrentStatus = Status.NotOnFire;

        // *** experimental testing ***
        _inOrder = true;
    }

    private void Update()
    {
        // point system move to burning bar
        if (!_points.IsUnityNull() && _points.enabled)
        {
            var temp = _points.transform.position; 
            if (temp.x > _targetPointsX && temp.y > _targetPointsY)
            {
                // _points.transform.position = Vector3.MoveTowards(temp,  GameManager.Instance.burnedPointsFireToMoveTowards.position , 0.2f);
                _points.transform.position = Vector3.MoveTowards(temp,  _targetPointsY * Vector3.up + 
                _targetPointsX * Vector3.right, 0.2f);
            }
            else
            {
                StartCoroutine(GameManager.Instance.FadeOut(_points));
            }
        }
        
        if (!CurrentStatus.Equals(Status.OnFire))
        {
            
            if (!_smoke.IsUnityNull() && _smoke.isPlaying && !isFireSource)
                _smoke.Stop();
            return;
        }
        
        if (!GameManager.Instance.GetBurningSound().isPlaying)
            GameManager.Instance.GetBurningSound().Play();
        
        // all the things under this line means they are on FIRE!!
        // remember this!!
        if (!_smoke.IsUnityNull() && _smoke.isStopped && !isFireSource)
            _smoke.Play();
        
        
        _timeUntilBurnOut -= Time.deltaTime;
        _currentTimerToAddReleaseFire += Time.deltaTime;

        if (_currentHealth <= 0f)
        {
            // the object is completely burnt!
            CurrentStatus = Status.FinishedBurning;
            BurnedOutEffectAndPoints();

            if (isFireSource) return;
            // release all fires
            foreach (var fire in _firesOnImage)
            {
                GameManager.Instance.FireObjectPool.Release(fire);
            }

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
                foreach (var (distance, otherFlameScript) in
                         _objectsAroundUsSorted) // todo make sure we go from negative (small) to positive (big)
                {
                    if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;

                    // we want this, try to burn it!, else move to another one.. (distance < _maxDistanceFromOrigin)
                    var chanceFromDistance = (int)((1 - distance / _maxDistanceFromOrigin) * 100);

                    otherFlameScript.TryToBurn(chanceFromDistance,
                        chanceFromBurnTime); // true if it burned, false if not
                }
            }
            else
            {
                foreach (var otherFlameScript in _objectsAroundUs)
                {
                    if (!otherFlameScript.CurrentStatus.Equals(Status.NotOnFire)) continue;

                    // we want this, try to burn it!, else move to another one..
                    otherFlameScript.TryToBurn(50, chanceFromBurnTime); // true if it burned, false if not
                }
            }
        }


        _passedTimeForCooldown += Time.deltaTime;

        if (_gettingExtinguished)
        {
            GettingExtinguished();
        }
        else
        {
            _currentHealth -=
                Time.deltaTime; // we decrease the health only if we are on fire without being extinguished
            if (!isFireSource)
            {
                _healthBarObj.value = _currentHealth;
                if (!_isImageVisible)
                {
                    _isImageVisible = true;
                    MakeImageInvisibleOrVisible(_healthBarImage, true);
                    MakeImageInvisibleOrVisible(_healthBarBorder, true);
                }
            }
        }


        if (!isFireSource)
        {
            if (_currentTimerToAddReleaseFire >= _addFireTime)
            {
                // add a new fire object to image
                _releaseFireTime = _addFireTime;
                _addFireTime += timeBetweenAddingNewFire;
                AddFireOnObject();
            }
            else if (_currentTimerToAddReleaseFire < _releaseFireTime)
            {
                // remove a fire object from image
                _addFireTime = _releaseFireTime;
                _releaseFireTime -= timeBetweenAddingNewFire;

                var fire = _firesOnImage.LastOrDefault();
                if (fire.IsUnityNull()) return;
                _firesOnImage.Remove(fire);
                GameManager.Instance.FireObjectPool.Release(fire);
            }
        }
    }


    private void BurnedOutEffectAndPoints()
    {
        _spriteRenderer.color = Color.black;
        MakeImageInvisibleOrVisible(_healthBarImage, false);
        MakeImageInvisibleOrVisible(_healthBarBorder, false);
        
        // points
        GameManager.Instance.numBurnedPoints += numOfPoints;
        GameManager.Instance.UpdateCurBurnedPoints();
        _points.text = "" + numOfPoints;
        StartCoroutine(GameManager.Instance.FadeIn(_points));
        if (isHouse)
        {
            GameManager.NumBurnedHouses++;
        }
    }

    private void ChangingSpriteColorBecauseOfFireOrWater()
    {
        _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, Color.black, Time.deltaTime / _timeUntilBurnOut);
        if (isFireSource) return;
        if (CurrentStatus.Equals(Status.OnFire))
            _healthBarImage.color = Color.Lerp(_healthBarImage.color, Color.red, Time.deltaTime / _timeUntilBurnOut);
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
        if (isFireSource)
        {
            _objectsAroundUsSorted.Clear();     // make sure they are empty :3
            GetFlammableObjectsAroundUs();
        }

        CurrentStatus = Status.OnFire;
    }

    public void SetSelfWatering(bool status)
    {
        _gettingExtinguished = status;
    }

    private void GettingExtinguished()
    {
    
        _timeUntilBurnOut += Time.deltaTime * extinguishingSpeed;
        _currentTimerToAddReleaseFire -= Time.deltaTime * extinguishingSpeed;
        if (!(_timeUntilBurnOut >= initialTimeUntilBurnOut)) return;
        // we watered the object
        if (GameManager.Instance.GetBurningSound().isPlaying)
            GameManager.Instance.GetBurningSound().Stop();
        
        initialTimeUntilBurnOut = _timeUntilBurnOut;
        CurrentStatus = Status.NotOnFire;
        if (isFireSource)
        {
            // _objectsAroundUsSorted.Clear();
        }

        else
        {
            // release all fires
            foreach (var fire in _firesOnImage)
            {
                GameManager.Instance.FireObjectPool.Release(fire);
            }
        }
    }

    private void AddFireOnObject()
    {
        var fireObject = GameManager.Instance.FireObjectPool.Get();
        _firesOnImage.Add(fireObject);
        Vector3 randomPoint;

        if (isHouse)
        {
            randomPoint = new Vector3(Random.Range(_bounds.min.x, _bounds.max.x),
                Random.Range(_bounds.min.y, _bounds.max.y), 0);
        }
        else
        {
            randomPoint =
                (Vector2) _bounds.center + Random.insideUnitCircle * Mathf.Min(_bounds.extents.x, _bounds.extents.y);
        }

        fireObject.transform.position = randomPoint;
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
            if (res.Equals(this) || res.isFireSource) continue;

            _objectsAroundUs.Add(res);

            // sort by distance of colliders (MAYBE DELETE IT LATER)
            // var tempDistance = Physics2D.Distance(_collider, col).distance;
            // if (_objectsAroundUsSorted.ContainsKey(tempDistance))
            var distance = Physics2D.Distance(_collider, col).distance;
            while (_objectsAroundUsSorted.ContainsKey(distance))
            {
                distance += 0.01f;
            }
            _objectsAroundUsSorted.Add(distance, res);
        }
    }
    
    private void InitializePointsText()
    {
        var pointsPos = _bounds.center + Vector3.up * _bounds.extents.y + 2 * Vector3.right;
        var temp = Instantiate(Resources.Load("Points"), pointsPos, Quaternion.identity,
            GameManager.Instance.barsParent) as GameObject;
        if (temp == null)
            throw new NullReferenceException("There is not Prefab for points text!");
        _points = temp.GetComponent<TextMeshProUGUI>();
        _points.enabled = false;
    }

    private void InitializeHealthBar()
    {
        // get size of _sprite
        var healthBarPos = _bounds.center + Vector3.up * _bounds.extents.y;

        var healthBar = Instantiate(Resources.Load("HealthBar"), healthBarPos,
            Quaternion.identity, GameManager.Instance.barsParent) as GameObject;

        // putting the healthbar at specific location with specific size!
        var rectTransform = healthBar.GetComponent<RectTransform>();
        var percentageToKeepInMind = rectTransform.lossyScale.x;
        rectTransform.position += (Vector3.up * GameManager.HealthBarHeight / 2) * percentageToKeepInMind;
        var healthBarWidthPercentage =
            (GameManager.HealthBarWidthPercentage / 100 * _bounds.size.x) / percentageToKeepInMind;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthBarWidthPercentage);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GameManager.HealthBarHeight);

        // some initializings
        _healthBarObj = healthBar.GetComponent<Slider>();
        _healthBarObj.maxValue = initialTimeUntilBurnOut;
        _healthBarObj.value = initialTimeUntilBurnOut;
        _healthBarImage = _healthBarObj.transform.GetChild(0).GetComponent<Image>();
        _healthBarBorder = _healthBarObj.transform.GetChild(1).GetComponent<Image>();

        // making the healthbar hidden until first shot:
        _isImageVisible = false;
        MakeImageInvisibleOrVisible(_healthBarImage, false);
        MakeImageInvisibleOrVisible(_healthBarBorder, false);
    }

    private static void MakeImageInvisibleOrVisible(Image image, bool makeVisible)
    {
        float visibility = makeVisible ? 1 : 0;
        image.color = new Color(image.color.r, image.color.g, image.color.b, visibility);
    }

    private Vector2 GetSizeOfArea()
    {
        var objectSize = _collider.bounds.size;
        objectSize += 2 * objectSize / ratioOfRadiusBySize;
        return objectSize;
    }

}