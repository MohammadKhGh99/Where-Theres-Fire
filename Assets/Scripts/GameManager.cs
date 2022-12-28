using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    
    // *** Time of the Game variables **** ///
    [SerializeField] private TextMeshProUGUI timerText;         // timer counting down until the game is over
    [SerializeField] private float gameTimeInSeconds;         // the game time in seconds
    private const float DEFAULT_GAME_TIME = 5.0f * 60.0f;       // 5 Minutes
    private float _gameTimer;
    
    
    //  **** Housing.. ****
    [SerializeField] private GameObject housesParent;           // where to store the houses Parent
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
    
    // **** Start and End Pages ****
    private GameObject _startGameCanvas;
    private GameObject _waterWonCanvas;
    private GameObject _fireWonCanvas;
    private Image _imageStartGame;
    private Image _imageWaterWon;
    private Image _imageFireWon;
    
    // the game is running or not
    public static bool IsGameRunning;
    
    // **** PARENTS ****
    public GameObject extinguisherWaterStream;
    public GameObject shotWaterStream;
    
    // this is a declaration for the singleton, maybe it's not needed, keep it for now. 

    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    
    //  *** Constants ***:
    // buildingStatus
    public const string NORMAL = "Normal";
    public const string BURNING = "Burning";
    public const string BURNED = "Burned";
    public const string WATERING = "Watering";
    public const string WAS_BURNED = "Was Burned";
    
    // WaterBullet Stasuses
    public enum WaterBulletStatus
    {
        Start, Enlarge, Shoot, Decrease, Done
    }
    
    // Building mask
    public LayerMask BuildingsMask { get; private set; }

    // Burned Building
    public static int NumBurnedBuildings = 0;
    
    // public static Canvas healthBarParent;

    
    

    // **** "Molotov" pool and functions ****
    public const float MolotovCooldownTime = 0f;
    
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
    
    
    // **** "WaterBullet" pool and functions ****
    public ObjectPool<WaterBullet> WaterBulletPool =
        new (CreateWaterBullet, OnGetWaterBullet, OnReleaseWaterBullet, OnDestroyWaterBullet, false, 15, 20);
    private static WaterBullet CreateWaterBullet()
    {
        var waterBullet = Instantiate(Resources.Load("WaterBullet")) as GameObject;
        var script = waterBullet.GetComponent<WaterBullet>();
        script.currentStatus = WaterBulletStatus.Start;
        return script;

    }
    private static void OnGetWaterBullet(WaterBullet waterBullet)
    {
        waterBullet.FakeStart();
    }
    private static void OnReleaseWaterBullet(WaterBullet waterBullet)
    {
        waterBullet.FakeRelease();
    }
    private static void OnDestroyWaterBullet(WaterBullet waterBullet)
    {
        Destroy(waterBullet.gameObject);
    }
    
    
    // **** "FireMolotov" pool and functions ****
    public ObjectPool<FireMolotov> FireMolotovPool =
        new (CreateFireMolotov, OnGetFireMolotov, OnReleaseFireMolotov, OnDestroyFireMolotov, false, 15, 20);
    private static FireMolotov CreateFireMolotov()
    {
        var fireMolotov = Instantiate(Resources.Load("FireMolotov")) as GameObject;
        return fireMolotov.GetComponent<FireMolotov>();
    }
    private static void OnGetFireMolotov(FireMolotov fireMolotov)
    {
        fireMolotov.FakeStart();
    }
    private static void OnReleaseFireMolotov(FireMolotov fireMolotov)
    {
        fireMolotov.FakeRelease();
    }
    private static void OnDestroyFireMolotov(FireMolotov fireMolotov)
    {
        Destroy(fireMolotov.gameObject);
    }
    
    void Start()
    {
        // // ** set the game timer **//
        // _gameTimer = gameTimeInSeconds != 0 ? gameTimeInSeconds : DEFAULT_GAME_TIME;
        //
        // // ** the game didn't start yet **
        // IsGameRunning = false;
        //
        // BuildingsMask =  LayerMask.GetMask("Building");
        //
        // _housesPosBackUp = controlHousesPos ? housesPositions : _housesPosBackUp;
        // for (int i = 0; i < _housesPosBackUp.Length; i++)
        // {
        //     var temp = _housesPosBackUp[i].Split(',');
        //     float x = float.Parse(temp[0]), y = float.Parse(temp[1]);
        //     var curPos = new Vector3(x, y, 0);
        //     Instantiate(Resources.Load("Building"), curPos, Quaternion.identity, housesParent.transform);
        //     
        // }
        
        InitializeGame();
        
        // getting the canvas and images of the start and end screens...
        _startGameCanvas = transform.GetChild(0).gameObject;
        _imageStartGame = _startGameCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();
        
        _waterWonCanvas = transform.GetChild(1).gameObject;
        _imageWaterWon = _waterWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        _fireWonCanvas = transform.GetChild(2).gameObject;
        _imageFireWon = _fireWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();
    }

    private void InitializeGame()
    {
        _gameTimer = gameTimeInSeconds != 0 ? gameTimeInSeconds : DEFAULT_GAME_TIME;
        
        // ** the game didn't start yet **
        IsGameRunning = false;
        
        BuildingsMask =  LayerMask.GetMask("Building");

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
        UpdateTheTimeOfTheGame();

        // ** if esc pressed while in game, we go to start page
        if (Input.GetKey(KeyCode.Escape) && IsGameRunning)
        {
            StartCoroutine(FadeOut(_imageFireWon));
            StartCoroutine(FadeOut(_imageWaterWon));
            StartCoroutine(FadeIn(_imageStartGame));
            IsGameRunning = false;
            return;
        }
        
        // ** to quit the game press esc
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameRunning) 
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // ** to start the game press any key to start
        // if (Input.anyKeyDown && !IsGameRunning)
        // {
        //     InitializeGame();
        //     StartCoroutine(FadeOut(_imageStartGame));
        //     IsGameRunning = true;
        // }

        // if (NumBurnedBuildings >= 1 && IsGameRunning)
        // {
        //     StartCoroutine(FadeIn(_imageFireWon));
        //     IsGameRunning = false;
        // }
        
        
    }

    // timer countdown
    private float _currentSeconds = 0;
    private void UpdateTheTimeOfTheGame()
    {
        if (!IsGameRunning)
            return;
        // counter to end of game 
        // todo: IT doesn't stop.
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
    
    // function that make fade out effect on image
    private IEnumerator FadeOut(Image image)
    {
        Color c = image.color;

        for (float i = 0.25f; i >= 0; i -= Time.deltaTime)
        {
            image.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        image.gameObject.SetActive(false);
    }

    // function that make fade in effect on image
    private IEnumerator FadeIn(Image image)
    {
        image.gameObject.SetActive(true);
        Color c = image.color;
        for (float i = 0; i <= 0.25f; i += Time.deltaTime)
        {
            image.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        image.color = new Color(c.r, c.g, c.b, 1);
    }
    
}
