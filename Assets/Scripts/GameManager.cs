using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    // *** Time of the Game variables **** ///
    [SerializeField] private TextMeshProUGUI timerText; // timer counting down until the game is over
    [SerializeField] private float gameTimeInSeconds; // the game time in seconds
    [SerializeField] private float winPercent = 0.5f;
    private const float DEFAULT_GAME_TIME = 5.0f * 60.0f; // 5 Minutes
    private float _gameTimer;

    // *** Players ***
    [SerializeField] private GameObject fireMan;
    [SerializeField] private GameObject extinguisher;
    private Extinguisher _extinguisherMan;
    private FireMan _fireMan;
    

    //  **** Housing.. ****
    [SerializeField] private GameObject housesParent; // where to store the houses Parent
    [SerializeField] private GameObject barsParent;
    [SerializeField] private bool controlHousesPos;

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

    // the game is over or not
    public static bool IsGameOver;


    // this is a declaration for the singleton, maybe it's not needed, keep it for now. 

    public new static GameManager Instance;

    private void Awake()
    {
        Instance = this;
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
        Start,
        Enlarge,
        Shoot,
        Decrease,
        Done
    }


    // Building mask
    public LayerMask HousesMask { get; private set; }

    // Burned Building
    public static int NumBurnedHouses = 0;
    
    // num houses
    private static int _numHouses = 0;

    // Types of Houses we have
    private string[] _housesTypes = { "House1", "House2" };
    private HouseManager[] _houses;


    // **** TileMap and Tiles ****
    public Tilemap GroundBaseTilemap;
    public RuleTile WaterTile;


    // **** "Molotov" pool and functions ****
    public const float MolotovCooldownTime = 0f;

    public ObjectPool<Molotov> MolotovPool =
        new(CreateMolotov, GetMolotov, ReturnMolotov, DestroyMolotov, false, 5, 7);

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
    // Pool
    public ObjectPool<WaterBullet> WaterBulletPool =
        new(CreateWaterBullet, OnGetWaterBullet, OnReleaseWaterBullet, OnDestroyWaterBullet, false, 15, 20);

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

    // Functions
    public static void SetTileAndUpdateNeighbors(Vector3 worldPosition, Tilemap myTilemap, TileBase newTile)
    {
        
        // set tile,
        Vector3Int gridPosition = myTilemap.WorldToCell(worldPosition);
        myTilemap.SetTile(gridPosition, newTile);
        myTilemap.RefreshTile(gridPosition);

        // check with neighbours
        var topTilePos = gridPosition + new Vector3Int(0, 1, 0);
        var bottomTilePos = gridPosition + new Vector3Int(0, -1, 0);
        var leftTilePos = gridPosition + new Vector3Int(-1, 0, 0);
        var rightTilePos = gridPosition + new Vector3Int(1, 0, 0);
        var topLeftTilePos = gridPosition + new Vector3Int(-1, 1, 0);
        var topRightTilePos = gridPosition + new Vector3Int(1, 1, 0);
        var bottomLeftTilePos = gridPosition + new Vector3Int(-1, -1, 0);
        var bottomRightTilePos = gridPosition + new Vector3Int(1, -1, 0);


        // Get the tiles around the modified tile
        TileBase topTile = myTilemap.GetTile(topTilePos);
        TileBase bottomTile = myTilemap.GetTile(bottomTilePos);
        TileBase leftTile = myTilemap.GetTile(leftTilePos);
        TileBase rightTile = myTilemap.GetTile(rightTilePos);
        TileBase topLeftTile = myTilemap.GetTile(topLeftTilePos);
        TileBase topRightTile = myTilemap.GetTile(topRightTilePos);
        TileBase bottomLeftTile = myTilemap.GetTile(bottomLeftTilePos);
        TileBase bottomRightTile = myTilemap.GetTile(bottomRightTilePos);

        // Check if the tiles around the modified tile need to be updated
        if (topTile != newTile)
        {
            myTilemap.SetTile(topTilePos, newTile);
            myTilemap.RefreshTile(topTilePos);
        }

        if (bottomTile != newTile)
        {
            myTilemap.SetTile(bottomTilePos, newTile);
            myTilemap.RefreshTile(bottomTilePos);
        }

        if (leftTile != newTile)
        {
            myTilemap.SetTile(leftTilePos, newTile);
            myTilemap.RefreshTile(leftTilePos);
        }

        if (rightTile != newTile)
        {
            myTilemap.SetTile(rightTilePos, newTile);
            myTilemap.RefreshTile(rightTilePos);
        }

        if (topLeftTile != newTile)
        {
            myTilemap.SetTile(topLeftTilePos, newTile);
            myTilemap.RefreshTile(topLeftTilePos);
        }

        if (topRightTile != newTile)
        {
            myTilemap.SetTile(topRightTilePos, newTile);
            myTilemap.RefreshTile(topRightTilePos);
        }

        if (bottomLeftTile != newTile)
        {
            myTilemap.SetTile(bottomLeftTilePos, newTile);
            myTilemap.RefreshTile(bottomLeftTilePos);
        }

        if (bottomRightTile != newTile)
        {
            myTilemap.SetTile(bottomRightTilePos, newTile);
            myTilemap.RefreshTile(bottomRightTilePos);
        }
    }

    public static void SetTile(Vector3 worldPosition, Tilemap thisTilemap, TileBase newTile)
    {
        Vector3Int gridPosition = thisTilemap.WorldToCell(worldPosition);
        
        if (thisTilemap.HasTile(gridPosition))
        {
            if (thisTilemap.GetTile(gridPosition) == newTile) return;
        }

        thisTilemap.SetTile(gridPosition, newTile);

        // Check if the tile is a Rule Tile
        RuleTile ruleTile = newTile as RuleTile;
        if (!ruleTile.IsUnityNull())
        {
            // Refresh the tile to apply the Rule Tile's rules
            thisTilemap.RefreshTile(gridPosition);
        }
        
    }
    

    // **** "FireMolotov" pool and functions ****
    public ObjectPool<FireMolotov> FireMolotovPool =
        new(CreateFireMolotov, OnGetFireMolotov, OnReleaseFireMolotov, OnDestroyFireMolotov, false, 15, 20);

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
        _extinguisherMan = extinguisher.GetComponent<Extinguisher>();
        _fireMan = fireMan.GetComponent<FireMan>();
        var housesParentTransform = housesParent.transform;
        _numHouses = housesParentTransform.childCount;
        _houses = new HouseManager[_numHouses];
        for (int i = 0; i < _numHouses; i++)
        {
            _houses[i] = housesParentTransform.GetChild(i).GetComponent<HouseManager>();
            var healthBar = Instantiate(Resources.Load("HealthBar"), _houses[i].transform.position + Vector3.up * 2,
                    Quaternion.identity, barsParent.transform) as GameObject;
            if (healthBar == null)
            {
                throw new NullReferenceException("There is no Health Bar in the Prefabs!");
            }
            _houses[i].SetHealthBar(healthBar);
        }
        InitializeGame();

        // getting the canvas and images of the start and end screens...
        _startGameCanvas = transform.GetChild(0).gameObject;
        _imageStartGame = _startGameCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        _waterWonCanvas = transform.GetChild(1).gameObject;
        _imageWaterWon = _waterWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        _fireWonCanvas = transform.GetChild(2).gameObject;
        _imageFireWon = _fireWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        // transform.Find("Field").transform.Find("Buildings");
    }

    private void InitializeGame()
    {
        _gameTimer = gameTimeInSeconds != 0 ? gameTimeInSeconds : DEFAULT_GAME_TIME;
        // _extinguisherMan.StartGame();
        // _fireMan.StartGame();
        
        // ** the game didn't start yet **
        IsGameRunning = false;

        HousesMask = LayerMask.GetMask("Houses");

        _housesPosBackUp = controlHousesPos ? housesPositions : _housesPosBackUp;
        if (!controlHousesPos)
        {
            // foreach (var house in _houses)
            // {
            //     house.StartGame();
            // }
            return;
        }
        foreach (var t in _housesPosBackUp)
        {
            var temp = t.Split(',');
            float x = float.Parse(temp[0]), y = float.Parse(temp[1]);
            var curPos = new Vector3(x, y, 0);
            var houseType = _housesTypes[Random.Range(0, 2)];
            Instantiate(Resources.Load(houseType), curPos, Quaternion.identity, housesParent.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTheTimeOfTheGame();

        // ** if esc pressed while in game, we go to start page
        if ((Input.GetKey(KeyCode.Space) && IsGameOver) || (Input.GetKey(KeyCode.Escape) && IsGameRunning))
        {
            SceneManager.LoadScene("Main");
            // StartCoroutine(FadeOut(_imageFireWon));
            // StartCoroutine(FadeOut(_imageWaterWon));
            // StartCoroutine(FadeIn(_imageStartGame));
            IsGameRunning = false;
            IsGameOver = false;
            // InitializeGame();
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
        if (Input.GetKey(KeyCode.Space) && !IsGameRunning)     // Input.anyKeyDown
        {
            // InitializeGame();
            StartCoroutine(FadeOut(_imageStartGame));
            IsGameRunning = true;
        }

        // check which player won
        if(!IsGameOver)
        {
            // ** water man won **
            if (_currentSeconds >= _gameTimer && (float)NumBurnedHouses / _numHouses < winPercent)
            {
                StartCoroutine(FadeOut(_imageStartGame));
                StartCoroutine(FadeIn(_imageWaterWon));
                IsGameRunning = false;
                IsGameOver = true;
                return;
            }

            // ** fire man won **
            if (_currentSeconds < _gameTimer && (float)NumBurnedHouses / _numHouses >= winPercent)
            {
                StartCoroutine(FadeOut(_imageStartGame));
                StartCoroutine(FadeIn(_imageFireWon));
                IsGameRunning = false;
                IsGameOver = true;
            }
        }
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