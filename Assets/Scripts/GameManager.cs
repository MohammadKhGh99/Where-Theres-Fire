using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private TextMeshProUGUI timerText;
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
    
    public const string NORMAL = "Normal";
    public const string BURNING = "Burning";
    public const string BURNED = "Burned";
    public const string WATERING = "Watering";
    public const string WAS_BURNED = "Was Burned";

    private float _currentSeconds = 0;
    
    

    // molotov pool and functions
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

    
    // SplashBullet pool and functions
    public const float SplashBulletCooldownTime = 3f;
    
    public ObjectPool<SplashBullet> SplashBulletPool =
        new (CreateSplashBullet, GetSplashBullet, ReturnSplashBullet, DestroySplashBullet, false, 5, 7);
    private static SplashBullet CreateSplashBullet()
    {
        var splashBullet = Instantiate(Resources.Load("SplashBullet") as GameObject);
        return splashBullet.GetComponent<SplashBullet>();
    }
    private static void GetSplashBullet(SplashBullet splashBullet)
    {
        splashBullet.gameObject.SetActive(true);
        splashBullet.FakeStart();
    }
    private static void ReturnSplashBullet(SplashBullet splashBullet)
    {
        splashBullet.gameObject.SetActive(false);
    }
    private static void DestroySplashBullet(SplashBullet splashBullet)
    {
        Destroy(splashBullet);
    }

    
    void Start()
    {
        timerText.text = "05:00";
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
        _currentSeconds += Time.deltaTime;
        int minutes = ((int)(_currentSeconds / 60)) % 60;
        int seconds = ((int)_currentSeconds) % 60;
        minutes = 5 - minutes - 1;
        seconds = 59 - seconds;
        if (minutes == 1 && seconds == 0)
            timerText.color = Color.red;
        
        // Showing the elapsed time in game
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    
    
}
