using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : MonoBehaviour
{
    [SerializeField] private float horseSpeed = 5f;
    private Flammable _flammable;
    private Transform _t;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;

    private bool horseStartRuninning;

    void Start()
    {
        _flammable = GetComponent<Flammable>();
        _t = GetComponent<Transform>();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        horseStartRuninning = false;
    }

    void Update()
    {
        if (_flammable.CurrentStatus == Flammable.Status.OnFire)
        {
            horseStartRuninning = true;
        }

        if (horseStartRuninning)
        {
            var isLeft = _sr.flipX ? -1 : 1;
            // _rb.velocity = horseSpeed * isLeft *  Vector2.left;
            transform.Translate(horseSpeed * isLeft * Time.deltaTime *  Vector2.left);

        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_flammable.CurrentStatus == Flammable.Status.OnFire)
        {
            if (col.gameObject.TryGetComponent(out Flammable res))
            {
                res.TryToBurn(100, 100);
            }
        }
    }
}
