using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Rigidbody2D _rb;

    // bomb ability
    [SerializeField] private float bombTravelDistance;
    [SerializeField] private float bombPower;
    [SerializeField] private float timeToReachTarget;
    private Vector3 _bombDropPos;


    private Vector3 shadowStartPosition;
    private Vector3 shadowStartScale;
    private Transform _shadowT;
    
    private Vector3 startPosition;
    private Vector3 startScale;

    private Vector3 throwingDirection;
    private float throwStartTime;

    private float oscillationHeight;
    private float oscillationSpeed;
    private float oscillationPhase;



    private bool _hasBeenShot = false;
    private bool _reachedTarget = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        var tra = transform;
        _shadowT = tra.Find("Shadow");
        shadowStartPosition = _shadowT.transform.position;
        shadowStartScale = _shadowT.transform.localScale;
        startScale = tra.localScale;
    }
    
    private void Update()
    {
        if (_hasBeenShot && !_reachedTarget)
        {
            // calculate time until we reach the actual time to reach target
            float timePassed = Time.time - throwStartTime;
            float throwProgress = timePassed / timeToReachTarget;
            if (throwProgress >= 1)
            {
                _reachedTarget = true;
                return;
            }
            
            // get oscillation
            oscillationPhase = (oscillationPhase + Time.deltaTime * oscillationSpeed) % 1;
            
            // move position to target
            Vector3 bombPosition = Vector3.Lerp(startPosition, _bombDropPos, throwProgress);
            bombPosition.y += Mathf.Sin(oscillationPhase * Mathf.PI * 2) * oscillationHeight;
            _rb.MovePosition(bombPosition);
            
            float bombScale = 1 + Mathf.Abs(Mathf.Sin(oscillationPhase * Mathf.PI * 2)) * 0.5f;

            transform.localScale = Vector3.one * bombScale;

            // Calculate the scale of the shadow based on the oscillation phase
            float shadowScale = shadowStartScale.x + Mathf.Abs(Mathf.Sin(oscillationPhase * Mathf.PI * 2)) * 0.5f;

            // Set the scale of the shadow
            _shadowT.transform.localScale = new Vector3(shadowScale, shadowStartScale.y, shadowScale);
        }
    }
    
    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
        transform.position = startPosition;
    }
    
    public Vector3 GetBombDropPos(Vector3 throwDirection)
    {
        throwingDirection = throwDirection;
        _bombDropPos = bombTravelDistance * throwDirection + transform.position;
        return _bombDropPos;
    }
    
    public IEnumerator Shoot()
    {
        _hasBeenShot = true;
        throwStartTime = Time.time;
        yield return new WaitUntil(() => _reachedTarget);
        
    }
}
