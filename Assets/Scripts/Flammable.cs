using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : MonoBehaviour
{
    // initializing variables
    [SerializeField] private float initialTimeUntilBurnOut;
    [SerializeField] private float initialChanceOfInflammation;
    
    // current status
    enum Status { NotOnFire, OnFire, FinishedBurning }
    private Status _currentStatus;

    // onFire variables
    private float _timeUntilBurnOut;


    // NotOnFire variables
    private float _currentChanceOfInflammation;


    private void Start()
    {
        _timeUntilBurnOut = initialTimeUntilBurnOut;
        _currentChanceOfInflammation = initialChanceOfInflammation;
        
        _currentStatus = Status.NotOnFire;
        
    }
    
    private void Update()
    {
        
        
        
        
    }


    public void TryToGetBurned(float chanceFromDistance)
    {
        
        
        
        
    }
    
    
    public void SetObjectOnFire()
    {
        // i guess we need to collect all the objects around us now, and we randomly burn an object from the list
        
        
        _currentStatus = Status.OnFire;
    }

}
