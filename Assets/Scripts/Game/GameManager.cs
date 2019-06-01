using System.Collections.Generic;
using Game;
using Player;
using Settings;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private static GameManager _instance;


    [Header("Levels")]
    [SerializeField] private List<LevelSettings> LevelSettings;

    [Header("Settings")]
    [SerializeField] private GameSettings GameSettings;
    
    [SerializeField] private GaugeSettings GradesSettings;
    public Gauge Grades    { get; private set; }
    
    [SerializeField] private GaugeSettings HappinessSettings;
    public Gauge Happiness { get; private set; }
    
    [SerializeField] private GaugeSettings FatigueSettings;
    public Gauge Fatigue   { get; private set; }
    
    public static int CurrentLevel { get; private set; }
    private GameObject _instantiatedLevel;

    private GameUIController _uiController;

    private const string PlayerPath = "Prefabs/Characters/Player";
    private PlayerController _playerController;

    
    void Awake() {
        
        if (_instance != null)
            Destroy(gameObject);

        _instance = this;

        _uiController = GetComponentInChildren<GameUIController>();
        if (_uiController == null)
            Debug.LogError("GameManager => GameUIController not found");
    }

    public void Start() {
        InitGauges();
        InitLevel();
        InitPlayer();
    }

    private void InitGauges() {
        Grades = new Gauge(GradesSettings, UpdateGauge, GameOver, ToggleWarning);
        Happiness = new Gauge(HappinessSettings, UpdateGauge, GameOver, ToggleWarning);
        Fatigue = new Gauge(FatigueSettings, UpdateGauge, GameOver, ToggleWarning);
    }

    private void InitLevel() {
        if (_instantiatedLevel != null)
            Destroy(_instantiatedLevel);

        if (CurrentLevel < LevelSettings.Count) {
            _instantiatedLevel = Instantiate(LevelSettings[CurrentLevel].LevelPrefab, transform);
        } else {
          Debug.LogError($"GameManager => LevelSettings not found for index {CurrentLevel}");  
        }
    }

    private void InitPlayer() {
        GameObject player = Instantiate(Resources.Load<GameObject>(PlayerPath));
        if (player != null)
            _playerController = player.GetComponentInChildren<PlayerController>();
    }

    private void UpdateGauge(float value, float max, Gauge.GaugeType type) {
        _uiController.ProgressGauge(value / max, type);
    }

    private void ToggleWarning(bool active, Gauge.GaugeType type) {
        _uiController.ToggleWarningGauge(active, type);
    }
    
    private void GameOver(string reason) {
        _playerController.SetState(State.GameOver);
        _uiController.EnableGameOver(reason);
    }
    
    public static GameManager Get() {
        return _instance;
    }

    public static GameSettings GetSettings() {
        return _instance.GameSettings;
    }
    
    public static void LoadNextLevel() {
        CurrentLevel++;
        SceneManager.LoadScene("Game");
    }
}
