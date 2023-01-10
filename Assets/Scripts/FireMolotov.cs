using UnityEngine;

public class FireMolotov : MonoBehaviour
{
    [SerializeField] private float targetScale = 4f;
    [SerializeField] private float timeToEnlarge = 0.2f;
    [SerializeField] private float timeToReduce = 0.1f;
    
    private Transform _t;
    private readonly Vector3 _startScale = Vector3.one;
    private float _elapsedTime;
    private bool _hasInitialized;

    private Status _currentStatus;

    enum Status { Pause, Burn, Extinguish }
    
    
    private void Update()
    {
        if (_currentStatus.Equals(Status.Burn))
        {
            _elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsedTime / timeToEnlarge);
            _t.localScale = Vector3.Lerp(_t.localScale, _startScale * targetScale, t);
            if (t >= 1)
            {
                _currentStatus = Status.Pause;
                _elapsedTime = 0f;
            }
        }
        else if (_currentStatus.Equals(Status.Extinguish))
        {
            _elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsedTime / timeToReduce);
            _t.localScale = Vector3.Lerp(_t.localScale, _startScale / 100.0f, t);
            if (t >= 1)
            {
                GameManager.Instance.FireMolotovPool.Release(this);
            }
        }
    }

    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = GetComponent<Transform>();
        }

        _t.localScale = _startScale;
        _elapsedTime = 0f;
        _currentStatus = Status.Pause;
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
