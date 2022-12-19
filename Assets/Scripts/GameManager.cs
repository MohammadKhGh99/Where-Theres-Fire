using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject housesParent;
    [SerializeField] private bool controlHousesPos = true;

    [SerializeField] private string[] housesPositions =
    {
        "-28.75,12.5", "-14.25,12.5", "0.25,12.5", "14.75,12.5", "29,12.5",
        "-28.75,-0.5", "-14.25,-0.5", "0.25,-0.5", "14.75,-0.5", "29,-0.5", 
        "-28.75,-13.5", "-14.25,-13.5", "0.25,-13.5", "14.75,-13.5", "29,-13.5"
    };
    
    private string[] _housesPosBackUp =
    {
        "-28.75,12.5", "-14.25,12.5", "0.25,12.5", "14.75,12.5", "29,12.5",
        "-28.75,-0.5", "-14.25,-0.5", "0.25,-0.5", "14.75,-0.5", "29,-0.5", 
        "-28.75,-13.5", "-14.25,-13.5", "0.25,-13.5", "14.75,-13.5", "29,-13.5"
    };
    
    
    // this is a declaration for the singleton, maybe it's not needed, keep it for now. (if made problem DELETE)
    protected GameManager(){}
    
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
    public const float BombCooldownTime = 3f;

    public ObjectPool<Bomb> BombPool =
        new (CreateBomb, GetBomb, ReturnBomb, DestroyBomb, false, 5, 7);
    private static Bomb CreateBomb()
    {
        var bomb = Instantiate(Resources.Load("Bomb") as GameObject);
        return bomb.GetComponent<Bomb>();
    }
    private static void GetBomb(Bomb bomb)
    {
        bomb.gameObject.SetActive(true);
        bomb.FakeStart();
    }
    private static void ReturnBomb(Bomb bomb)
    {
        bomb.gameObject.SetActive(false);
    }
    private static void DestroyBomb(Bomb bomb)
    {
        Destroy(bomb);
    }


    void Start()
    {
        _housesPosBackUp = controlHousesPos ? housesPositions : _housesPosBackUp;
        for (int i = 0; i < _housesPosBackUp.Length; i++)
        {
            var temp = _housesPosBackUp[i].Split(',');
            float x = float.Parse(temp[0]), y = float.Parse(temp[1]);
            var curPos = new Vector3(x, y, 0);
            Instantiate(Resources.Load("Building"), curPos, Quaternion.identity, housesParent.transform);
            
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
