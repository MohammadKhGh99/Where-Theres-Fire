using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGun : MonoBehaviour
{
    private WaterBullet _currentWaterBullet;

    public void CreateWaterStream()
    {
        _currentWaterBullet = GameManager.instance.WaterBulletPool.Get();
    }

    public void EnlargeWaterStream(Vector3 playerCurrentPos, Vector3 playerLookAtDirection, Vector3 startPosition)
    {
        _currentWaterBullet.EnlargeBullet(playerCurrentPos,  playerLookAtDirection, startPosition);
    }
    
    public void ShootWaterStream()
    {
        _currentWaterBullet.ShootBullet();
    }
}
