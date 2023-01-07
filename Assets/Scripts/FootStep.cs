using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class FootStep : MonoBehaviour
{
    [SerializeField] private float legsWide;
    public float timeBeforeDisappear;
    private bool _hasInitialized;
    private Transform _t;

    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = transform;
        }

        var tempScale = _t.localScale;
        tempScale.x = Mathf.Abs(tempScale.x);
        _t.localScale = tempScale;
        _hasInitialized = true;
    }

    public void SetStep(Vector3 position, Vector2 direction, WetShoes.Legs step)
    {
        // todo make sure the direction is for directional and it's 01 only
        // Direction
        // (0,1) - rotation 0
        // (1,0) - rotation 270
        // (0,-1) - rotation 180
        // (-1,0) - rotation 90
        
        // fix rotation
        var rotationAngle = Mathf.Abs(direction.x)>0.2 ?(180 + direction.x * 90) :(90 - direction.y * 90) ;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

        var currentLeg = step.Equals(WetShoes.Legs.Right) ? 1 : -1;
        
        // fix position
        var tempPos = position;
        tempPos.x += currentLeg * legsWide;
        _t.position = tempPos;
        
        // fix leg direction (left leg or right leg)
        var currentScale = _t.localScale;
        currentScale.x *= currentLeg;
        _t.localScale = currentScale;
    }
}
