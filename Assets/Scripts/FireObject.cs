using UnityEngine;

public class FireObject : MonoBehaviour
{
    [SerializeField] private Vector2 targetScaleMultiply = new (2f, 3f);
    [SerializeField] private float enlargingSpeed = 0.5f;

    private Transform _t;

    private Vector3 _startScale;

    // flag to not initialize things again and again after pooling
    private bool _hasInitialized = false;
    private float _elapsedTime;


    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = GetComponent<Transform>();
            _startScale = _t.localScale;
            _hasInitialized = true;
        }

        _elapsedTime = 0;
        _t.localScale = _startScale;
        gameObject.SetActive(true);
    }

    public void FakeRelease()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            _elapsedTime += enlargingSpeed * Time.deltaTime;
            _t.localScale = Vector3.Lerp(_t.localScale, _startScale * targetScaleMultiply, _elapsedTime);
        }
    }
}
