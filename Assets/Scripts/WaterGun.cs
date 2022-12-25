using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGun : MonoBehaviour
{
    private float _waitingBetweenShots;             // this var represents the time you should wait between each shot, it will be calculated at the start
    private float _waterGunLastShotTime = 0f;

    
    void Start()
    {
        _waitingBetweenShots = CalculateWaitingBetweenShots();
    }



    public IEnumerator Shoot(Vector3 gunPos, Vector3 shootingDirection)
    {
        if (Time.time >= _waterGunLastShotTime + _waitingBetweenShots)
        {
            _waterGunLastShotTime = Time.time;     //reset cooldown
            
            var waterSquare = GameManager.instance.WaterSquarePool.Get();
            yield return waterSquare.Shoot(gunPos, shootingDirection);
            
            // when we finish with the waterSquare
            var waterSquareDiePos = waterSquare.GetWaterSquareDiePos();
            GameManager.instance.WaterSquarePool.Release(waterSquare);
        }
    }
    
    private float CalculateWaitingBetweenShots()
    {
        // this function is used to shoot a square water from the gun with good aligning, we don't want shots to be over each other
        // this will run only 1 time at the start.
        
        // what we do is: taking the width of square, calculate the time the WaterSquare will pass it all, and this is the time to shoot another WaterSquare
        var tempWaterSquare = GameManager.instance.WaterSquarePool.Get();
        var (scale, travelDistance, travelTime) = tempWaterSquare.GetWaterSquareInformation();
        var wsWidth = scale.x;
        var travelSpeed = travelDistance / travelTime;
        var waitingTime = wsWidth / travelSpeed;
        GameManager.instance.WaterSquarePool.Release(tempWaterSquare);
        return waitingTime;

    }
}
