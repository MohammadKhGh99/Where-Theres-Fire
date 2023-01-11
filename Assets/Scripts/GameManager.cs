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
    private Vector3 _initialTextScale;
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
    [SerializeField] private GameObject housesParent;    // where to store the houses Parent
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

    // *** game important flags ***
    public static bool IsGameRunning;    // the game is running or not
    public static bool IsGameOver;     // the game is over or not

    // this is a declaration for the singleton, maybe it's not needed, keep it for now. 

    public new static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }


    //  *** Constants ***:
    // buildingStatus
    public enum HouseStatus
    {
        Normal,
        Burning,
        Burned,
        Watering,
        WasBurned
    }

    // WaterBullet Stasuses
    public enum WaterBulletStatus
    {
        Start,
        Enlarge,
        Shoot,
        Decrease,
        Done
    }


    // Houses mask
    public LayerMask HousesMask { get; private set; }
    public LayerMask BordersMask { get; private set; }

    // Burned Building
    public static int NumBurnedHouses = 0;

    // num houses
    private static int _numHouses = 0;

    // Types of Houses we have
    private string[] _housesTypes = { "House1", "House2" };
    private HouseManager[] _houses;
    public Slider burnedHousesBar;
    [SerializeField] private float maxBurnedPoints = 10;


    // **** TileMap and Tiles ****
    public Tilemap GroundBaseTilemap; 
    public Tilemap WaterFireTilemap; 
    public RuleTile WaterTile;
    public TileBase GroundTile;


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
        var temp = Instantiate(Resources.Load("HealthBar"), new Vector3(-21, 0, 0), Quaternion.identity,
            barsParent.transform) as GameObject;
        if (temp == null)
            throw new NullReferenceException("You cannot build new burned points bar, there is no such bar in Prefabs!");
        temp.transform.Rotate(Vector3.forward, 90);
        burnedHousesBar = temp.GetComponent<Slider>();
        burnedHousesBar.maxValue = maxBurnedPoints;
        burnedHousesBar.value = 0;
        var transform1 = burnedHousesBar.transform;
        var scale = transform1.localScale;
        transform1.localScale = new Vector3(scale.x + 5, scale.y, 0);

        // ** houses **
        var housesParentTransform = housesParent.transform;
        _numHouses = housesParentTransform.childCount;
        _houses = new HouseManager[_numHouses];
        for (var i = 0; i < _numHouses; i++)
        {
            _houses[i] = housesParentTransform.GetChild(i).GetComponent<HouseManager>();
            var healthBar = Instantiate(Resources.Load("HealthBar"), _houses[i].transform.position + Vector3.up * 2.3f,
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
        if (!_startGameCanvas.activeInHierarchy)
            _startGameCanvas.SetActive(true);

        _waterWonCanvas = transform.GetChild(1).gameObject;
        _imageWaterWon = _waterWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        _fireWonCanvas = transform.GetChild(2).gameObject;
        _imageFireWon = _fireWonCanvas.GetComponent<Transform>().GetChild(0).GetComponent<Image>();

        _initialTextScale = timerText.transform.localScale;
    }

    private void InitializeGame()
    {
        _gameTimer = gameTimeInSeconds != 0 ? gameTimeInSeconds : DEFAULT_GAME_TIME;
        // _extinguisherMan.StartGame();
        // _fireMan.StartGame();

        // ** the game didn't start yet **
        IsGameRunning = false;

        HousesMask = LayerMask.GetMask("Houses");
        BordersMask = LayerMask.GetMask("Borders");

        _housesPosBackUp = controlHousesPos ? housesPositions : _housesPosBackUp;
        if (!controlHousesPos)
            return;

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
            StartCoroutine(FadeIn(_imageStartGame));
            SceneManager.LoadScene("Main");
            IsGameRunning = false;
            IsGameOver = false;
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
        if (Input.GetKey(KeyCode.Space) && !IsGameRunning) // Input.anyKeyDown
        {
            StartCoroutine(FadeOut(_imageStartGame));
            IsGameRunning = true;
        }

        // check which player won
        if (!IsGameOver)
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
            var allBurned = _currentSeconds < _gameTimer && NumBurnedHouses == _numHouses;
            var winPercentReached = _currentSeconds >= _gameTimer && (float)NumBurnedHouses / _numHouses >= winPercent;
            if (allBurned || winPercentReached)
            {
                StartCoroutine(FadeOut(_imageStartGame));
                StartCoroutine(FadeIn(_imageFireWon));
                IsGameRunning = false;
                IsGameOver = true;
            }
        }
    }

    // timer countdown
    private float _currentSeconds;
    private float _pulsingTimer;

    private void UpdateTheTimeOfTheGame()
    {
        if (!IsGameRunning)
            return;
        // counter to end of game 
        // todo: IT doesn't stop.
        _pulsingTimer += Time.deltaTime;
        _currentSeconds += Time.deltaTime;
        var minutes = (int)(_currentSeconds / 60) % 60;
        var seconds = (int)_currentSeconds % 60;
        minutes = (int)_gameTimer / 60 - minutes - 1;
        seconds = 59 - seconds;
        if (minutes == 1 && seconds == 0)
            timerText.color = Color.red;

        if (minutes < 1 && seconds is <= 59 and > 0 && _pulsingTimer >= 0.5f)
        {
            _pulsingTimer = 0;
            timerText.enabled = !timerText.enabled;
        }

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

    public IEnumerator FadeOut(SpriteRenderer spriteRenderer)
    {
        Color c = spriteRenderer.color;

        for (float i = 0.25f; i >= 0; i -= Time.deltaTime)
        {
            spriteRenderer.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        spriteRenderer.enabled = false;
        // image.gameObject.SetActive(false);
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

    public IEnumerator FadeIn(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.enabled = true;
        Color c = spriteRenderer.color;
        for (float i = 0; i <= 0.25f; i += Time.deltaTime)
        {
            spriteRenderer.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        spriteRenderer.color = new Color(c.r, c.g, c.b, 1);
    }
}