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
    private Animator _animator;

    private bool horseStartRuninning;

    void Start()
    {
        _flammable = GetComponent<Flammable>();
        _t = GetComponent<Transform>();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        horseStartRuninning = false;
    }

    void Update()
    {
        if (_t.position.x is <= -23 or >= 23 && GameManager.Instance.GetHorseSound().isPlaying)
            GameManager.Instance.GetHorseSound().Stop();
        if (_flammable.CurrentStatus == Flammable.Status.OnFire)
        {
            if (!GameManager.Instance.GetHorseSound().isPlaying)
                GameManager.Instance.GetHorseSound().Play();
            horseStartRuninning = true;
        }

        if (horseStartRuninning)
        {
            var isLeft = _sr.flipX ? -1 : 1;
            // _rb.velocity = horseSpeed * isLeft *  Vector2.left;
            if (!_animator.enabled)
                _animator.enabled = true;
            // _animator.SetBool("Running", true);
            _t.Translate(horseSpeed * isLeft * Time.deltaTime *  Vector2.left);

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
