using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class WetShoes : MonoBehaviour
{
    [SerializeField] private int numOfWetCells;
    [SerializeField] private float legsWide;
    [SerializeField] private float timeBeforeDisappear;

    private Transform _t;
    private bool _isWetShoes;
    private int _currentNumOfWetCellsMade;
    
    // adding a second footstep when we are standing still
    private bool _secondLegInserted;
    private float _timerForSecondLeg;
    private const float FixedTimeToInsertSecondLeg = 1f;

    // previous cycle info
    private Vector3Int _previousPos;
    private Vector3 _previousDirection;

    private List<FootStep> _stepsToStartCoroutine = new List<FootStep>();
    public enum Legs { Left, Right }
    private Legs _nextLegToUse;
    void Start()
    {
        _isWetShoes = false;
        _currentNumOfWetCellsMade = 0;
        _nextLegToUse = Legs.Right;
        _t = GetComponent<Transform>();
    }

    void Update()
    {
        var currentPos= GameManager.Instance.WaterFireTilemap.WorldToCell(_t.position);
        if(!GameManager.Instance.WaterFireTilemap.HasTile(currentPos)) return;     // dunno just keep it
        var currentTile = GameManager.Instance.WaterFireTilemap.GetTile(currentPos);

        if (currentTile.Equals(GameManager.Instance.WaterTile))
        {
            _isWetShoes = true;
            _currentNumOfWetCellsMade = 0;
            
            // release any steps that was made before
            if (_stepsToStartCoroutine.Count > 0)
            {
                foreach (var footStepToRelease in _stepsToStartCoroutine)
                {
                    StartCoroutine(FootStepTime(footStepToRelease));
                }
                _stepsToStartCoroutine.Clear();
            }
        }
        else // not on water tile
        {
            if (!_isWetShoes) return;   // if not wet shoes, nothing to do here
            if (_previousPos.Equals(currentPos)) // we are on same position as last time
            {
                if (!_secondLegInserted && _timerForSecondLeg > FixedTimeToInsertSecondLeg)
                {
                    // we can now insert the second leg
                    var secondFootStep = _footStepPool.Get();
                    secondFootStep.SetStep(_t.position, _previousDirection, _nextLegToUse, legsWide);
                    _nextLegToUse = _nextLegToUse.Equals(Legs.Right) ? Legs.Left : Legs.Right;
                    _stepsToStartCoroutine.Add(secondFootStep);
                    _secondLegInserted = true;
                }
                else
                {
                    _timerForSecondLeg += Time.deltaTime;
                }
            }
            else
            {
                // we moved to another tile, which is NOT WATER TILE
                // get footstep direction
                _previousDirection = Vector3.Normalize(currentPos - _previousPos);

                // setup the new footstep
                var footStep = _footStepPool.Get();
                footStep.SetStep(_t.position, _previousDirection, _nextLegToUse, legsWide);
                _nextLegToUse = _nextLegToUse.Equals(Legs.Right) ? Legs.Left : Legs.Right;
                _stepsToStartCoroutine.Add(footStep);

                // add 1 to the currentNumOfWetCellsMade
                _currentNumOfWetCellsMade += 1;
                if (_currentNumOfWetCellsMade > numOfWetCells)
                {
                    _isWetShoes = false;
                }

                // release any steps that was made before
                if (_stepsToStartCoroutine.Count > 0)
                {
                    foreach (var footStepToRelease in _stepsToStartCoroutine)
                    {
                        StartCoroutine(FootStepTime(footStepToRelease));
                    }
                    _stepsToStartCoroutine.Clear();
                }


                // reset second legs variables
                _secondLegInserted = false;
                _timerForSecondLeg = 0;
            }
        }
        
        _previousPos = currentPos;
    }


    private IEnumerator FootStepTime(FootStep footStep)
    {
        yield return new WaitForSeconds(timeBeforeDisappear);
        _footStepPool.Release(footStep);
    }
    
    
    private ObjectPool<FootStep> _footStepPool =
        new(CreateFootStep, GetFootStep, ReturnFootStep, DestroyFootStep, false, 24, 36);

    private static FootStep CreateFootStep()
    {
        var footStep = Instantiate(Resources.Load("FootStep") as GameObject);
        return footStep.GetComponent<FootStep>();
    }

    private static void GetFootStep(FootStep footStep)
    {
        footStep.gameObject.SetActive(true);
        footStep.FakeStart();
    }

    private static void ReturnFootStep(FootStep footStep)
    {
        footStep.gameObject.SetActive(false);
    }

    private static void DestroyFootStep(FootStep footStep)
    {
        Destroy(footStep);
    }
}
