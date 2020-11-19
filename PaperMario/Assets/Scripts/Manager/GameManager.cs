using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { PlayState,DialogState,MenuState,FadeState}

public class GameManager : MonoBehaviour {

    #region Variables

    public GameState CurrentState;

    [Header("Game Debug at Start")]
    public bool debugCodeChecker;
    [Tooltip("Will only work if Debug Code Checker Is True, returns a log stating if a certain area failed or was sucessful")]
    public bool debugLogReportBack;

    [Header("Player Prefab")]
    public GameObject player;

    [Header("Partner Prefabs")]
    public GameObject currentPartner;
    public GameObject[] partners;

    [Header("Entity Movement Values")]
    [Tooltip("Walking Speed")]
    public float speed;
    [Tooltip("Jump speed")]
    public float jumpSpeed;
    [Tooltip("The flip speed of each entity")]
    public float flipSpeed;

    [Header("Entity Ground Check Requirements")]
    public LayerMask isGroundLayer;
    public float groundCheckRadius;

    [Header("Entity Obstacle Mask")]
    public LayerMask obstacleMask;

    //To work with the level manager
    [System.NonSerialized]
    public LevelManager currentLevel = null;

    //UI
    public GeneralPlayerUI GeneralPlayerUI = null;

    //Battle
    bool _inBattle;
    public List<Skills> allSkills;
    //[System.NonSerialized]
    public List<Bestiary> enemiesForBattle;
    [System.NonSerialized]
    public CombatManager CM;
    [System.NonSerialized]
    public PlayerSelection playerSelection;
    EnemyController _currentEnemyInBattle;

    //ObjectPoolReference
    public ObjectPoolingManager objPoolManager;

    #endregion

    #region Singleton/Awake Function

    static GameManager _instance = null;

    public static GameManager instance
    {
        get { return _instance; }
        set { _instance = value; }
    }

    void Awake()
    {
        //Singleton Design Pattern
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        allSkills = new List<Skills>();
        Skills[] toBePassed = Resources.LoadAll<Skills>("Skills");
        for (int i = 0; i < toBePassed.Length; i++)
        {
            allSkills.Add(toBePassed[i]);
        }

        if(debugCodeChecker == true)
        {
            DebugMasterFunction();
        }
    }

    #endregion

    void Start()
    {
        _inBattle = false;
    }

    #region Getter/Setters

    public bool inBattle
    {
        get { return _inBattle; }
    }

    public EnemyController currentEnemyInBattle
    {
        get { return _currentEnemyInBattle; }
        set
        {
            _currentEnemyInBattle = value;
        }
    }

    public int MAX_AMOUNT_OF_UNITS_ON_FIELD
    {
        get { return 5; }
    }

    #endregion

    #region CombatManager

    public void BattleSceneEntered(List<Bestiary> bestiary)
    {
        if(inBattle == false)
        {
            enemiesForBattle = bestiary;

            _inBattle = true;//This happens early due to the fact that all entites will use this variable to update themselves
            SceneManager.LoadScene("TestBattle", LoadSceneMode.Additive);
            currentLevel.PauseEntitiesForBattle();
            Debug.Log("BattleSceneEntered");
        }
    }

    public void BattleSceneExited()
    {
        if(inBattle == true)
        {
            _inBattle = false;
            SceneManager.UnloadSceneAsync("TestBattle");
            currentLevel.ResumeEntitiesFromBattle();
            //Enemy enters dead state
        }
    }

    #endregion

    #region Debug Code Checker

    void DebugMasterFunction()
    {
        DebugSkillsPriorites();
        DebugElementalChartOutput();
    }

    /// <summary>
    /// Checks if there are multiple moves in the same priority spot and returns errors if there are any
    /// </summary>
    void DebugSkillsPriorites()
    {
        bool test = false;
        for (int i = 0; i < allSkills.Count; i++)
        {
            for (int j = 0; j < allSkills.Count; j++)
            {
                if(allSkills[i] == allSkills[j] || allSkills[i].skillType != allSkills[j].skillType)
                {
                    continue;
                }

                if(allSkills[i].priorityListing == allSkills[j].priorityListing)
                {
                    Debug.LogError(allSkills[i].name + " Priority Number is the exact same as " + allSkills[j].name + " priority, This will cause erros in the list organization");
                    test = true;
                }
            }
        }

        if(debugLogReportBack == true)
        {
            if(test == false)
            {
                Debug.Log("DebugSkillsPriorites Successful");
            }
            else
            {
                Debug.LogWarning("DebugSkillsPriorites Failed");
            }
        }
    }

    void DebugElementalChartOutput()
    {
        float testfloat = ElementalTypeManager.ReturnDamageMultiplier(ElementalType.Normal, ElementalType.Rock);
        ElementalType attacktypeTest;
        ElementalType defensetypeTest;

        for (int i = 0; i < System.Enum.GetNames(typeof(ElementalType)).Length; i++)
        {
            for (int j = 0; j < System.Enum.GetNames(typeof(ElementalType)).Length; j++)
            {
                attacktypeTest = (ElementalType)i;
                defensetypeTest = (ElementalType)j;

                Debug.Log("Test Attack " + attacktypeTest + " against defense type " + defensetypeTest + " = " + ElementalTypeManager.ReturnDamageMultiplier(attacktypeTest, defensetypeTest));
            }
        }
    }

    #endregion

    #region from mobile game

    ////Singleton Design Pattern
    //static GameManager _instance = null;

    //public GameObject playerPrefab;
    //public AudioMananger AM;

    //int _score;
    //public Text scoreText;

    //int _coin;
    //public Text coinText;

    //float _distance;
    //public Text distanceText;

    //int _highScore;
    //public Text highScoreText;

    //[SerializeField]
    //float _gameFlowSpeed;

    //float _dashMultiplier;

    ////Player Prefs
    //public float musicVolume;
    //public float sfxVolume;
    //int musicToggleSave;
    //int sfxToggleSave;



    //void Awake()
    //{
    //    //Singleton Design Pattern
    //    if (instance)
    //    {
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        instance = this;
    //        DontDestroyOnLoad(this);
    //    }

    //    StartingValues();
    //}

    //private void Start()
    //{
    //    AM = AudioMananger.instance;
    //    MainMenuMusic();
    //    LoadGame();
    //}


    //public static GameManager instance
    //{
    //    get { return _instance; }
    //    set { _instance = value; }
    //}

    ////public void spawnPlayer(int spawnLocation)
    ////{
    ////    string spawnPointName = SceneManager.GetActiveScene().name + "_" + spawnLocation; //Level1_0

    ////    Transform spawnPointTransform = GameObject.Find(spawnPointName).GetComponent<Transform>();

    ////    if (playerPrefab && spawnPointTransform)
    ////    {
    ////        Instantiate(playerPrefab, spawnPointTransform.position, spawnPointTransform.rotation);
    ////    }
    ////    else
    ////    {
    ////        Debug.Log("GameManager Script : Missing Prefab or cant find transform.");
    ////    }

    ////}

    //public int score
    //{
    //    get { return _score; }
    //    set
    //    {
    //        _score = value;
    //        if (scoreText)
    //            scoreText.text = "Score: " + score.ToString("0");
    //    }

    //}

    //public int coin
    //{
    //    get { return _coin; }
    //    set
    //    {
    //        _coin = value;
    //        if (coinText)
    //            coinText.text = "x " + coin.ToString("0");
    //    }

    //}

    //public float distance
    //{
    //    get { return _distance; }
    //    set
    //    {
    //        _distance = value;
    //        if (distanceText)
    //            distanceText.text = "Distance: " + distance.ToString("00") + "m";
    //        //distanceText.text = "Distance Travelled: " + distance.ToString("00") + "m";
    //    }

    //}

    //public int highScore
    //{
    //    get { return _highScore; }
    //    set
    //    {
    //        _highScore = value;
    //        if (highScoreText)
    //            highScoreText.text = "HighScore: " + highScore.ToString("0");
    //    }

    //}

    //public float gameFlowSpeed
    //{
    //    get { return _gameFlowSpeed; }
    //    set { _gameFlowSpeed = value; }
    //}

    //public float dashMultiplier
    //{
    //    get { return _dashMultiplier; }
    //    set { _dashMultiplier = value; }
    //}

    //public void PlayerDied()
    //{
    //    SceneManager.LoadScene("GameOver");
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.GameOverMenuMusic);

    //    if (score > highScore)
    //    {
    //        highScore = score;
    //    }

    //    //scoreText.text = "Score: " + score.ToString("0");
    //    //highScoreText.text = "HighScore: " + highScore.ToString("0");

    //    SaveGame();
    //}

    //public void StartGame()
    //{
    //    SceneManager.LoadScene("Game");
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.GameMusic);
    //    StartingValues();
    //}

    //public void MainMenu()
    //{
    //    SceneManager.LoadScene("MainMenu");
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.StartMenuMusic);
    //    SaveGame();
    //    if (score > highScore)
    //    {
    //        highScore = score;
    //    }
    //    SaveGame();
    //}

    //public void QuitGame()
    //{
    //    Application.Quit();
    //}

    //public void Credits()
    //{
    //    //SceneManager.LoadScene("Credits");
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.CreditMusic);
    //}

    //public void HowToPlay()
    //{
    //    //SceneManager.LoadScene("HowToPlay");
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.HowToPlayMusic);
    //}

    //public void MainMenuMusic()
    //{
    //    AM.musicSource.Stop();
    //    AM.PlayMusic(AM.StartMenuMusic);
    //}

    //public void StartingValues()
    //{
    //    score = 0;
    //    coin = 0;
    //    distance = 0;
    //    gameFlowSpeed = 5f;
    //    dashMultiplier = 1;
    //}

    //// Save Game
    //public void SaveGame()
    //{
    //    PlayerPrefs.SetInt("HighScore", highScore);
    //    //int musicToggleSave;
    //    if (AM.musicToggleSwitch)
    //    {
    //        musicToggleSave = 1;
    //        //PlayerPrefs.SetInt("MusicToggle", 1);
    //    }
    //    else
    //    {
    //        musicToggleSave = 0;
    //        //PlayerPrefs.SetInt("MusicToggle", 0);
    //    }

    //    if (AM.sfxToggleSwitch)
    //    {
    //        sfxToggleSave = 1;
    //        //PlayerPrefs.SetInt("SFXToggle", 1);
    //    }
    //    else
    //    {
    //        sfxToggleSave = 0;
    //        //PlayerPrefs.SetInt("SFXToggle", 0);
    //    }

    //    PlayerPrefs.SetInt("MusicToggle", musicToggleSave);
    //    PlayerPrefs.SetInt("SFXToggle", sfxToggleSave);
    //    PlayerPrefs.SetFloat("MusicVolume", AM.musicVolume);
    //    PlayerPrefs.SetFloat("SFXVolume", AM.sfxVolume);
    //    PlayerPrefs.Save();
    //    //musicVolume = AM.musicVolume;
    //    //sfxVolume = AM.sfxVolume;
    //    //musicToggle = AM.musicToggleSwitch;
    //    //sfxToggle = AM.sfxToggleSwitch;
    //    //LoadSaveManager.SaveHighScore(this);
    //}

    //// Load Game
    //public void LoadGame()
    //{
    //    if (!PlayerPrefs.HasKey("HighScore"))
    //    {
    //        PlayerPrefs.SetInt("HighScore", 0);
    //    }
    //    else
    //    {
    //        highScore = PlayerPrefs.GetInt("HighScore");
    //    }

    //    if (!PlayerPrefs.HasKey("MusicToggle"))
    //    {
    //        PlayerPrefs.SetInt("MusicToggle", 1);
    //    }
    //    else
    //    {
    //        musicToggleSave = PlayerPrefs.GetInt("MusicToggle");
    //    }

    //    if (!PlayerPrefs.HasKey("SFXToggle"))
    //    {
    //        PlayerPrefs.SetInt("SFXToggle", 1);
    //    }
    //    else
    //    {
    //        sfxToggleSave = PlayerPrefs.GetInt("SFXToggle");
    //    }

    //    if (!PlayerPrefs.HasKey("MusicVolume"))
    //    {
    //        PlayerPrefs.SetInt("MusicVolume", 1);
    //        musicToggleSave = 1;
    //    }
    //    else
    //    {
    //        AM.musicVolume = PlayerPrefs.GetFloat("MusicVolume");
    //    }

    //    if (!PlayerPrefs.HasKey("SFXVolume"))
    //    {
    //        PlayerPrefs.SetInt("SFXVolume", 1);
    //        sfxToggleSave = 1;
    //    }
    //    else
    //    {
    //        AM.sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
    //    }

    //    if (musicToggleSave == 1)
    //    {
    //        AM.musicToggleSwitch = true;
    //    }
    //    else
    //    {
    //        AM.musicToggleSwitch = false;
    //    }

    //    if (sfxToggleSave == 1)
    //    {
    //        AM.sfxToggleSwitch = true;
    //    }
    //    else
    //    {
    //        AM.sfxToggleSwitch = false;
    //    }
    //    //GameStateData data = LoadSaveManager.LoadHighScore();
    //    //if(data != null)
    //    //{
    //    //    highScore = data.previousHighscore;
    //    //    AM.musicVolume = data.musicVolume;
    //    //    AM.sfxVolume = data.sfxVolume;
    //    //    AM.musicToggleSwitch = data.musicToggle;
    //    //    AM.sfxToggleSwitch = data.sfxToggle;
    //    //}
    //}

    #endregion

    #region from term 2 


    ////Singleton Design Pattern
    //static GameManager _instance = null;
    //public GameObject playerPrefab;


    //int _score;
    //public Text scoreText;

    //int _coin;
    //public Text coinText;

    //int _lives;
    //public Text livesText;


    //void Start()
    //{
    //    //Singleton Design Pattern
    //    if (instance)
    //    {
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        instance = this;
    //        DontDestroyOnLoad(this);
    //    }
    //    //DontDestroyOnLoad(gameObject);

    //    score = 0;
    //    coin = 0;
    //    lives = 3;
    //}

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        if (SceneManager.GetActiveScene().name == "Game_Over")
    //        {
    //            SceneManager.LoadScene("Main Menu");
    //            SoundManager.instance.PlayMusic(SoundManager.instance.StartMenuMusic, 0.7f);
    //            score = 0;
    //            coin = 0;
    //        }
    //        else if (SceneManager.GetActiveScene().name == "Winner")
    //        {
    //            SceneManager.LoadScene("Main Menu");
    //            SoundManager.instance.PlayMusic(SoundManager.instance.StartMenuMusic, 0.7f);
    //        }
    //    }

    //    if (livesText)
    //        livesText.text = "Lives x" + lives.ToString("00");
    //}

    //public void spawnPlayer(int spawnLocation)
    //{
    //    string spawnPointName = SceneManager.GetActiveScene().name + "_" + spawnLocation; //Level1_0

    //    Transform spawnPointTransform = GameObject.Find(spawnPointName).GetComponent<Transform>();

    //    if (playerPrefab && spawnPointTransform)
    //    {
    //        Instantiate(playerPrefab, spawnPointTransform.position, spawnPointTransform.rotation);
    //    }
    //    else
    //    {
    //        Debug.Log("GameManager Script : Missing Prefab or cant find transform.");
    //    }

    //}

    //public static GameManager instance
    //{
    //    get { return _instance; }
    //    set { _instance = value; }
    //}

    //public int score
    //{
    //    get { return _score; }
    //    set
    //    {
    //        _score = value;
    //        if (scoreText)
    //            scoreText.text = "Score: " + score.ToString("0");
    //    }

    //}

    //public int coin
    //{
    //    get { return _coin; }
    //    set
    //    {
    //        _coin = value;
    //        if (coinText)
    //            coinText.text = "x " + coin.ToString("00");
    //    }

    //}

    //public int lives
    //{
    //    get { return _lives; }
    //    set
    //    {
    //        _lives = value;
    //        if (livesText)
    //            livesText.text = "Lives x" + lives.ToString("00");
    //        Debug.Log("Lives: " + _lives);
    //    }

    //}

    //public void MarioDied()
    //{
    //    if (lives <= 0)
    //    {
    //        SceneManager.LoadScene("Game_Over");//Brings the Game to the game over map
    //        SoundManager.instance.musicSource.Stop();
    //        SoundManager.instance.PlayMusic(SoundManager.instance.GameOverMenuMusic, 1f, false);

    //    }
    //    else
    //    {
    //        SceneManager.LoadScene("Level1");
    //    }

    //}

    //public void StartGame()
    //{
    //    SceneManager.LoadScene("Level1");
    //    SoundManager.instance.musicSource.Stop();
    //    SoundManager.instance.PlayMusic(SoundManager.instance.Level1Music, 0.3f);
    //    lives = 3;// resets the lives to 3
    //    score = 0;
    //    coin = 0;
    //}

    //public void WinGame()
    //{
    //    SceneManager.LoadScene("Winner");
    //    SoundManager.instance.musicSource.Stop();
    //    SoundManager.instance.PlayMusic(SoundManager.instance.WinnerMusic, 1f, true);
    //    lives = 3;// resets the lives to 3
    //    GameObject.Find("Score_Text").GetComponent<Text>().text = "Score " + score;
    //}

    //public void QuitGame()
    //{
    //    Application.Quit();
    //}

    #endregion
}
