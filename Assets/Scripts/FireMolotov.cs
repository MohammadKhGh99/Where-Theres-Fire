using UnityEngine;

public class FireMolotov : MonoBehaviour
{
    [SerializeField] private float targetScale = 4f;
    [SerializeField] private float timeToEnlarge = 0.2f;
    [SerializeField] private float timeToReduce = 0.1f;
    private readonly Vector3 _startScale = Vector3.one;

    private Transform _t;
    private Flammable _flammable;

    private float _elapsedTime;

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

        _t.localScale = _startScale;
        _elapsedTime = 0f;
        _currentStatus = Status.Pause;
        _flammable.CurrentStatus = Flammable.Status.NotOnFire;
        gameObject.SetActive(true);
        _hasInitialized = true;
    }

    public void FakeRelease()
    {
        _currentStatus = Status.Pause;
        _elapsedTime = 0f;
        gameObject.SetActive(false);
    }

    public void Burn(Vector3 molotovDropPos)
    {
        _t.position = molotovDropPos;
        _currentStatus = Status.Burn;
        
    }

    public void Extinguish()
    {
        _currentStatus = Status.Extinguish;
    }
}
