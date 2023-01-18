using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class FootStep : MonoBehaviour
{
    private bool _hasInitialized;
    private Transform _t;
    private SpriteRenderer _sprite;
    // private Color _onWaterColor = new Vector4(68f, 116f, 132, 255);
    private readonly Color _onWaterColor = new (68f/255f, 116f/255f, 132f/255f, 255f/255f);
    private readonly Color _onGroundColor = new (106f/255f, 154f/255f, 198f/255f, 255f/255f);


    public void FakeStart()
    {
        if (!_hasInitialized)
        {
            _t = transform;
            _sprite = GetComponent<SpriteRenderer>();
        }

        var tempScale = _t.localScale;
        tempScale.x = Mathf.Abs(tempScale.x);
        _t.localScale = tempScale;
        _hasInitialized = true;
    }

    public void SetStep(Vector3 position, Vector2 direction, WetShoes.Legs step, float legsWide, bool isWaterTile)
    {
        // todo make sure the direction is for directional and it's 01 only
        // Direction
        // Right- (0,1) - rotation 0
        // Up   - (1,0) - rotation 270
        // Left - (0,-1) - rotation 180
        // Down - (-1,0) - rotation 90
        
        // fix rotation
        var rotationAngle = Mathf.Abs(direction.x)>0.2 ?(180 + direction.x * 90) :(90 - direction.y * 90) ;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

        var currentLeg = step.Equals(WetShoes.Legs.Right) ? 1 : -1;
        
        // fix position
        var tempPos = position;
        if(direction.x == 0)
            tempPos.x += currentLeg * legsWide;
        else
            tempPos.y += currentLeg * legsWide;
        
        _t.position = tempPos;
        
        // fix leg direction (left leg or right leg)
        var currentScale = _t.localScale;
        currentScale.x *= currentLeg;
        _t.localScale = currentScale;
        
        // fix color
        _sprite.color = isWaterTile ? _onWaterColor : _onGroundColor;
    }
}
