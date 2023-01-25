using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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
    // timer countdown
    private float _currentSeconds;
    private float _pulsingTimer;
    
    // ** bars parent (canvas)
    public Transform barsParent;

    // **** Start and End Pages ****
    // private GameObject startGameCanvas;
    // private GameObject waterWonCanvas;
    // private GameObject fireWonCanvas;
    private Image _imageStartGame;
    private Image _imageExtinguisherWon;
    private Image _imageFireWon;
    private Image _imageHowToPlay1;
    private Image _imageHowToPlay2;

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
    // WaterBullet Statuses
    public enum WaterBulletStatus
    {
        Start,
        Enlarge,
        Shoot,
        Decrease,
        Done
    }

    [SerializeField] public LayerMask forbiddenLayers;

    // Burned Building
    public static int NumBurnedHouses = 0;

    // num houses
    private static int _numHouses = 0;

    // Types of Houses we have
    private Flammable[] _houses;
    [FormerlySerializedAs("_burnedHousesBar")] public Slider burnedHousesBar;
    [SerializeField] private float maxBurnedPoints = 10;


    // *** HealthBar Info. ****
    public const float HealthBarHeight = 5f;
    public const float HealthBarWidthPercentage = 75f;


    // **** TileMap and Tiles ****
    public Tilemap GroundBaseTilemap; 
    public Tilemap WaterFireTilemap; 
    public RuleTile WaterTile;
    [SerializeField] private const float TilesDisappearTime = 10f; // time it will take to water tile to dissapear after being in game
    [SerializeField] private const float SprayToNextTileTime = 1f; // time it will take to water tile to dissapear after being in game
    private const float CheckingRatio = 0.25f;
    private float _checkingRatioTimer;

    // *** Burned Points ***
    public int numBurnedPoints;
    [SerializeField] private TextMeshProUGUI burnedPointsText;
    [SerializeField] public Transform burnedPointsFireToMoveTowards;
    
    // *** Sounds
    [SerializeField] private AudioSource burningSound;
    [SerializeField] private AudioSource molotovSound;
    [SerializeField] private AudioSource waterHoseSound;
    [SerializeField] private AudioSource horseSound;
    [SerializeField] private AudioSource clockTickingSound;
    
    // *** Buttons
    private Button _startButton;
    private Button _howToPlayButton;
    private Image _howToPlayImage;
    private Button _exitButton;
    public bool start, howToPlay, exit;
    
    // ** camera
    [SerializeField] private Camera mainCamera;
    // [SerializeField] private static GameObject bulletParent;

    // [SerializeField] private static GameObject waterBulletsParent;

    // **** "Molotov" pool and functions ****
    public const float MolotovCooldownTime = 1f;

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
    private static Dictionary<Vector3Int, float> _waterTileDisappearTimes = new();
    private static Dictionary<Vector3Int, float> _waterTileSprayNext = new();
    public static void SetTile(Vector3 worldPosition, Tilemap thisTilemap, TileBase newTile)
    {
        Vector3Int gridPosition = thisTilemap.WorldToCell(worldPosition);

        if (thisTilemap.HasTile(gridPosition))
        {
            if (thisTilemap.GetTile(gridPosition) == newTile)
            {
                _waterTileDisappearTimes[gridPosition] = Time.time + TilesDisappearTime;
                return;
            }
        }

        thisTilemap.SetTile(gridPosition, newTile);

        // Check if the tile is a Rule Tile
        RuleTile ruleTile = newTile as RuleTile;
        if (!ruleTile.IsUnityNull())
        {
            // Refresh the tile to apply the Rule Tile's rules
            thisTilemap.RefreshTile(gridPosition);
            // setting the timer for:
            _waterTileDisappearTimes[gridPosition] = Time.time + TilesDisappearTime;
        }
        else
        {
            // we want to delelte this waterTile, its unitynull so:
            if (_waterTileDisappearTimes.ContainsKey(gridPosition))
            {
                _waterTileDisappearTimes.Remove(gridPosition);
            }
        }
    }
    
    public static void SetTile(Vector3 worldPosition, Tilemap thisTilemap, TileBase newTile, Vector3 sprayDirection, float increasingValue = 1f)
    {
        Vector3Int gridPosition = thisTilemap.WorldToCell(worldPosition);

        if (thisTilemap.HasTile(gridPosition))
        {
            if (thisTilemap.GetTile(gridPosition) == newTile)
            {
                _waterTileDisappearTimes[gridPosition] = Time.time + TilesDisappearTime;
                _waterTileSprayNext[gridPosition] += increasingValue * Time.deltaTime;
                if (_waterTileSprayNext[gridPosition] >= SprayToNextTileTime)
                {
                    _waterTileSprayNext[gridPosition] = 0f;
                    var nextTilePos = worldPosition + sprayDirection;
                    SetTile(nextTilePos, thisTilemap, newTile, sprayDirection, increasingValue * 150);
                }
                return;
            }
        }

        thisTilemap.SetTile(gridPosition, newTile);

        // Check if the tile is a Rule Tile
        RuleTile ruleTile = newTile as RuleTile;
        if (!ruleTile.IsUnityNull())
        {
            // Refresh the tile to apply the Rule Tile's rules
            thisTilemap.RefreshTile(gridPosition);
            // setting the timer for:
            _waterTileDisappearTimes[gridPosition] = Time.time + TilesDisappearTime;
            _waterTileSprayNext[gridPosition] = Time.deltaTime;
        }
        else
        {
            // we want to delelte this waterTile, its unitynull so:
            if (_waterTileDisappearTimes.ContainsKey(gridPosition))
            {
                _waterTileDisappearTimes.Remove(gridPosition);
                _waterTileSprayNext[gridPosition] = 0;
            }
        }
    }
    private void UpdateWaterTiles()
    {
        if (!(Time.time >= _checkingRatioTimer)) return;
        _checkingRatioTimer = CheckingRatio + Time.time;
        HashSet<Vector3Int> toDelete = new ();
        foreach (var (gridPos, timeToDisappear) in _waterTileDisappearTimes)
        {
            if (!(timeToDisappear <= Time.time)) continue;
            // time for this tile to disappear
            Instance.WaterFireTilemap.SetTile(gridPos, null);
            toDelete.Add(gridPos);
        }

        foreach (var gridToDelete in toDelete)
        {
            _waterTileDisappearTimes.Remove(gridToDelete);
            _waterTileSprayNext[gridToDelete] = 0;
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
    
    // **** "FireObject" pool and functions ****
    public ObjectPool<FireObject> FireObjectPool =
        new(CreateFireObject, OnGetFireObject, OnReleaseFireObject, OnDestroyFireObject, false, 50, 70);

    private static FireObject CreateFireObject()
    {
        var fireObject = Instantiate(Resources.Load("FireObject")) as GameObject;
        return fireObject.GetComponent<FireObject>();
    }

    private static void OnGetFireObject(FireObject fireObject)
    {
        fireObject.FakeStart();
    }

    private static void OnReleaseFireObject(FireObject fireObject)
    {
        fireObject.transform.SetParent(null);
        fireObject.FakeRelease();
    }

    private static void OnDestroyFireObject(FireObject fireObject)
    {
        Destroy(fireObject.gameObject);
    }

    void Start()
    {
        burnedHousesBar.maxValue = maxBurnedPoints;
        burnedHousesBar.value = 0;

        _gameTimer = gameTimeInSeconds != 0 ? gameTimeInSeconds : DEFAULT_GAME_TIME;
        _currentSeconds = _gameTimer;

        // ** the game didn't start yet **
        IsGameRunning = false;
        IsGameOver = false;
        
        // // ** houses **
        // _numHouses = housesParent.childCount;
        // InitializeGame();

        // getting the canvas and images of the start and end screens...
        var startGameCanvas = transform.GetChild(0).gameObject;
        
        // Getting images of start, two ends and how to play screens
        _imageStartGame = startGameCanvas.transform.GetChild(0).GetComponent<Image>();
        
        _imageExtinguisherWon = startGameCanvas.transform.GetChild(1).GetComponent<Image>();
        _imageFireWon = startGameCanvas.transform.GetChild(2).GetComponent<Image>();
        _imageHowToPlay1 = startGameCanvas.transform.GetChild(3).GetComponent<Image>();
        _imageHowToPlay2 = startGameCanvas.transform.GetChild(4).GetComponent<Image>();
        
        // Getting the buttons on start game screen
        _startButton = _imageStartGame.transform.GetChild(0).gameObject.GetComponent<Button>();
        _howToPlayButton = _imageStartGame.transform.GetChild(1).gameObject.GetComponent<Button>();
        _exitButton = _imageStartGame.transform.GetChild(2).gameObject.GetComponent<Button>();

        _howToPlayImage = _howToPlayButton.GetComponent<Image>();

        if (!_imageStartGame.gameObject.activeInHierarchy)
            _imageStartGame.gameObject.SetActive(true);
        
        _initialTextScale = timerText.transform.localScale;
        burnedPointsText.text = numBurnedPoints + "/" + maxBurnedPoints;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTheTimeOfTheGame();
        UpdateWaterTiles(); // this function is for waterTiles to make them disappear after sometime, idk where to put it
        
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
        if (exit || (Input.GetKeyDown(KeyCode.Escape) && !IsGameRunning))
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // ** to start the game press any key to start
        if (start) // Input.GetKey(KeyCode.Space) && !IsGameRunning)
        {
            StartCoroutine(FadeOut(_imageStartGame));
            IsGameRunning = true;
            mainCamera.orthographicSize = 13f;
            start = false;
        }

        // if we clicked the how to play button
        if (howToPlay)
        {
            // first how to play screen appears
            if (!_imageHowToPlay1.gameObject.activeInHierarchy && !_imageHowToPlay2.gameObject.activeInHierarchy)
            {
                if (_imageStartGame.gameObject.activeInHierarchy)
                {
                    StartCoroutine(FadeOut(_imageStartGame));
                    _howToPlayImage.color = _howToPlayButton.colors.normalColor;
                    _howToPlayButton.interactable = true;
                }

                StartCoroutine(FadeIn(_imageHowToPlay1));
                IsGameRunning = false;
            }

            // second how to play screen appears
            if (_imageHowToPlay1.gameObject.activeInHierarchy && !_imageHowToPlay2.gameObject.activeInHierarchy 
                                                              && Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(FadeOut(_imageHowToPlay1));
                StartCoroutine(FadeIn(_imageHowToPlay2));
            }

            // the start screen appears again to choose what the user wants
            if (!_imageHowToPlay1.gameObject.activeInHierarchy && _imageHowToPlay2.gameObject.activeInHierarchy 
                                                               && Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(FadeOut(_imageHowToPlay2));
                StartCoroutine(FadeIn(_imageStartGame));
                for (int i = 0; i < _imageStartGame.transform.childCount; i++)
                {
                    _imageStartGame.transform.GetChild(i).gameObject.SetActive(true);
                }

                howToPlay = false;
            }
        }
        
        // check which player won
        if (!IsGameOver && IsGameRunning)
        {
            // ** water man won **
            if (_currentSeconds <= 0 && numBurnedPoints < maxBurnedPoints)
            {
                // StartCoroutine(FadeOut(_imageStartGame));
                mainCamera.orthographicSize = 20.2f;
                StartCoroutine(FadeIn(_imageExtinguisherWon));
                IsGameRunning = false;
                IsGameOver = true;
                DisableSounds();
                return;
            }

            // ** fire man won **
            if (numBurnedPoints >= maxBurnedPoints)
            {
                // StartCoroutine(FadeOut(_imageStartGame));
                mainCamera.orthographicSize = 20.2f;
                StartCoroutine(FadeIn(_imageFireWon));
                IsGameRunning = false;
                IsGameOver = true;
                DisableSounds();
            }
        }
    }

    private void UpdateTheTimeOfTheGame()
    {
        if (!IsGameRunning || _currentSeconds <= 0)
            return;
        // counter to end of game 
        // todo: IT doesn't stop.
        _pulsingTimer += Time.deltaTime;
        _currentSeconds -= Time.deltaTime;
        var minutes = (int)(_currentSeconds / 60);
        var seconds = (int)_currentSeconds % 60;
        
        if (((minutes == 1 && seconds == 0) || (minutes == 0)) && !timerText.color.Equals(Color.red))
        {
            timerText.color = Color.red;
            clockTickingSound.Play();
        }

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

        for (float i = 0.5f; i >= 0; i -= Time.deltaTime)
        {
            image.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        image.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut(SpriteRenderer spriteRenderer)
    {
        Color c = spriteRenderer.color;

        for (float i = 0.5f; i >= 0; i -= Time.deltaTime)
        {
            spriteRenderer.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        spriteRenderer.enabled = false;
    }
    
    public IEnumerator FadeOut(TextMeshProUGUI text)
    {
        Color c = text.color;

        for (float i = 0.5f; i >= 0; i -= Time.deltaTime)
        {
            text.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        text.enabled = false;
    }
    
    public IEnumerator FadeIn(TextMeshProUGUI text)
    {
        text.enabled = true;
        Color c = text.color;
        for (float i = 0; i <= 0.5f; i += Time.deltaTime)
        {
            text.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }
        text.color = new Color(c.r, c.g, c.b, 1);
    }

    // function that make fade in effect on image
    private IEnumerator FadeIn(Image image)
    {
        image.gameObject.SetActive(true);
        Color c = image.color;
        for (float i = 0; i <= 0.5f; i += Time.deltaTime)
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
        for (float i = 0; i <= 0.5f; i += Time.deltaTime)
        {
            spriteRenderer.color = new Color(c.r, c.g, c.b, i * 4);
            yield return null;
        }

        spriteRenderer.color = new Color(c.r, c.g, c.b, 1);
    }

    public void UpdateCurBurnedPoints()
    {
        burnedPointsText.text = numBurnedPoints + "/" + maxBurnedPoints;
        burnedHousesBar.value = numBurnedPoints;
    }

    public AudioSource GetBurningSound()
    {
        return burningSound;
    }
    
    public AudioSource GetWaterHoseSound()
    {
        return waterHoseSound;
    }
    
    public AudioSource GetMolotovSound()
    {
        return molotovSound;
    }

    public AudioSource GetHorseSound()
    {
        return horseSound;
    }

    public void HideButtons()
    {
        _startButton.gameObject.SetActive(false);
        _howToPlayButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
        // Destroy(_startButton);
        // Destroy(_howToPlayButton);
        // Destroy(_exitButton);
    }

    private void UnHideButtons()
    {
        _startButton.gameObject.SetActive(true);
        _howToPlayButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);
    }

    private void DisableSounds()
    {
        if (horseSound.isPlaying)
            horseSound.Stop();
        if (clockTickingSound.isPlaying)
            clockTickingSound.Stop();
        if (burningSound.isPlaying)
            burningSound.Stop();
        if (molotovSound.isPlaying)
            molotovSound.Stop();
        if (waterHoseSound.isPlaying)
            waterHoseSound.Stop();
    }
}