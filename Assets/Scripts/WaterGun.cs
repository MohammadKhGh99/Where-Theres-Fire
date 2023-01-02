using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGun : MonoBehaviour
{
    private WaterBullet _currentWaterBullet;
    private Vector3 _previousLookAtDirection;

    public void CreateWaterStream()
    {
        _currentWaterBullet = GameManager.Instance.WaterBulletPool.Get();
    }

    public void EnlargeWaterStream(Vector3 playerCurrentPos, Vector3 playerLookAtDirection, Vector3 startPosition)
    {
        if (_previousLookAtDirection.Equals(playerLookAtDirection))     // if we didn't change our direction, then enlarge
        {
            _currentWaterBullet.EnlargeBullet(playerCurrentPos,  playerLookAtDirection, startPosition);
        }
        else        // we changed our direction, shoot what you used to have, and create a new one, and enlarge it
        {
            _currentWaterBullet.ShootBullet();
            _currentWaterBullet = GameManager.Instance.WaterBulletPool.Get();
            _previousLookAtDirection = playerLookAtDirection; 
            _currentWaterBullet.EnlargeBullet(playerCurrentPos,  playerLookAtDirection, startPosition);
        }
    }
    
    public void ShootWaterStream()
    {
        _currentWaterBullet.ShootBullet();
    }
}
