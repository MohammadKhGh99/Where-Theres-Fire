using System;
using System.Collections;
using UnityEngine;

public class FireMolotov : MonoBehaviour
{
    [SerializeField] private float targetScale = 4f;
    [SerializeField] private float timeToEnlarge = 0.2f;
    [SerializeField] private float timeToReduce = 0.1f;
    private readonly Vector3 _startScale = Vector3.one;
    private readonly Vector2 _dummyLocation = new(-100f, -100f);

    private Transform _t;
    private Flammable _flammable;

    private float _elapsedTime;
    
    // ** steam
    private GameObject _steam;
    private Animator _steamAnimator;

    // status
    private Status _currentStatus;

    enum Status { Pause, Burn, Extinguish }
    
    // flag to not initialize things again and again after pooling
    private bool _hasInitialized;

    private void Update()
    {
        switch (_currentStatus)
        {
            case Status.Burn:
            {
                _elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(_elapsedTime / timeToEnlarge);
                _t.localScale = Vector3.Lerp(_t.localScale, _startScale * targetScale, t);
                if (t >= 1)
                {
                    // we reached final state, burn
                    _flammable.SetSelfOnFire();
                    //********************************
                    // todo here add coroutine of the time until fire molotov finish, when it's finish,
                    // todo change flammable status to not on fire, then change this status to extinguish
                    //*********************************
                    _currentStatus = Status.Pause;
                    _elapsedTime = 0f;
                }

                break;
            }
            case Status.Extinguish:
            {
                
                _flammable.CurrentStatus = Flammable.Status.NotOnFire;
                _elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(_elapsedTime / timeToReduce);
                _t.localScale = Vector3.Lerp(_t.localScale, _startScale / 100.0f, t);
                if (t >= 1)
                {
                    GameManager.Instance.FireMolotovPool.Release(this);
                }

                break;
            }
        }
    }

    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = GetComponent<Transform>();
            _flammable = GetComponent<Flammable>();
        }

        _steam = Instantiate(Resources.Load("Steam")) as GameObject;
        _steam.SetActive(false);
        
        _t.localScale = _startScale;
        _elapsedTime = 0f;
        _currentStatus = Status.Pause;
        _flammable.CurrentStatus = Flammable.Status.NotOnFire;
        _t.position = _dummyLocation;
        gameObject.SetActive(true);
        _hasInitialized = true;

        
        // _steamAnimator = _steam.GetComponent<Animator>();
        // _steamAnimator.enabled = false;
    }

    public void FakeRelease()
    {
        _currentStatus = Status.Pause;
        _elapsedTime = 0f;
        _t.position = _dummyLocation;
        gameObject.SetActive(false);
    }

    public void Burn(Vector3 molotovDropPos)
    {
        _t.position = molotovDropPos;
        _currentStatus = Status.Burn;
        _steam.transform.position = molotovDropPos - Vector3.up;
    }

    public void Extinguish()
    {
        _currentStatus = Status.Extinguish;
        StartCoroutine(ShowSteam());
        // _steamAnimator.enabled = true;
    }

    private IEnumerator ShowSteam()
    {
        _steam.SetActive(true);
        yield return new WaitForSeconds(1);
        _steam.SetActive(false);
    }
}
