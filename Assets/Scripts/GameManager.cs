using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameManager : Singleton<GameManager>
{
    // this is a declaration for the singleton, maybe it's not needed, keep it for now. (if made problem DELETE)
    protected GameManager(){}
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    // Constants:
    public const float RightAngle = 90;
    public const float LeftAngle = -90;
    public const float UpAngle = 180;
    public const float DownAngle = 0;
    
    
    // static function for moving direction
    public static Vector2 UpdateMoveDirection(Vector2 oldDirection, float xDirection, float yDirection)
    {
        // change the direction of the movement depends on the input direction and the old direction
        // the direction will be -1,0,1 for x and y

        var newDir = Vector2.zero;
        // if moveDirection != zero, then we were moving to one of the directions:
        if (oldDirection != Vector2.zero)
        {
            // we are moving in the X axis
            if (oldDirection.x != 0)
            {
                newDir.x = xDirection;
                // we continue moving in the x axis, else if we stopped moving in x axis, move toward y axis
                newDir.y = xDirection != 0 ? 0 : yDirection;
            }
            // we are moving in the Y axis

            else
            {
                newDir.y = yDirection;
                // we continue moving in the y axis, else if we stopped moving in y axis, move toward x axis
                newDir.x = yDirection != 0 ? 0 : xDirection;
            }
        }
        // if moveDirection is zero, we need to choose a direction (i guess we should go with X first)
        else
        {
            newDir.x = xDirection;
            // we move in the x axis, else move toward y axis
            newDir.y = xDirection != 0 ? 0 : yDirection;
        }

        return newDir;
    }
    
    // bomb pool and functions
    public const float MolotovCooldownTime = 3f;
    
    public ObjectPool<Molotov> MolotovPool =
        new (CreateMolotov, GetMolotov, ReturnMolotov, DestroyMolotov, false, 5, 7);
    private static Molotov CreateMolotov()
    {
        var molotov = Instantiate(Resources.Load("Molotov") as GameObject);
        return molotov.GetComponent<Molotov>();
    }
    private static void GetMolotov(Molotov molotov)
    {
        molotov.gameObject.SetActive(true);
        molotov.FakeStart();
    }
    private static void ReturnMolotov(Molotov molotov)
    {
        molotov.gameObject.SetActive(false);
    }
    private static void DestroyMolotov(Molotov molotov)
    {
        Destroy(molotov);
    }


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
