using System.Collections;
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
         LevelSettings settings = GetLevelSettings();
         if (settings == null) {
             Debug.LogError($"GameManager => Settings not found for Level: {CurrentLevel}");  
             return;
         }
        
         InitGauges(settings);
         InitLevel(settings);
         InitPlayer();

         StartCoroutine(ProgressGame(settings.Duration));
    }

    private void InitGauges(LevelSettings settings) {
        Grades = new Gauge(GradesSettings, UpdateGauge, GameOver, ToggleWarning);
        _uiController.SetGradeRequirementIndicator(settings.GradesRequirement / GradesSettings.MaxValue);
        
        Happiness = new Gauge(HappinessSettings, UpdateGauge, GameOver, ToggleWarning);
        Fatigue = new Gauge(FatigueSettings, UpdateGauge, GameOver, ToggleWarning);
    }

    private void InitLevel(LevelSettings settings) {
        if (_instantiatedLevel != null)
            Destroy(_instantiatedLevel);

        if (settings.LevelPrefab != null) {
            _instantiatedLevel = Instantiate(settings.LevelPrefab, transform);
        } else {
          Debug.LogError($"GameManager => Prefab not found for Level: {settings.Name}");  
        }
    }

    private void InitPlayer() {
        GameObject player = Instantiate(Resources.Load<GameObject>(PlayerPath));
        if (player != null)
            _playerController = player.GetComponentInChildren<PlayerController>();
    }
    
    private IEnumerator ProgressGame(float duration) {
        float elapsed = 0;

        _uiController.UpdateGameProgressBar(0);
            
        while (elapsed < duration && _playerController.CurrentState != State.GameOver) {
            elapsed += Time.deltaTime;
            _uiController.UpdateGameProgressBar(Mathf.Lerp(0, 1, elapsed / duration));
            yield return new WaitForEndOfFrame();
        }

        if (_playerController.CurrentState != State.GameOver) {
            _uiController.UpdateGameProgressBar(1);
            bool victory = Grades.Value >= GetLevelSettings().GradesRequirement;
            GameOver(victory, "You didn't pass your exams !");
        }
    }

    private void GameOver(bool victory, string reason) {
        if (_playerController.CurrentState != State.GameOver) {
            _playerController.SetState(State.GameOver);

            if (victory) {
                Debug.Log("Show Victory UI");
            } else {
                _uiController.EnableGameOver(reason);
            }
        }
    }
    
    private void UpdateGauge(float value, float max, Gauge.GaugeType type) {
        _uiController.ProgressGauge(value / max, type);
    }

    private void ToggleWarning(bool active, Gauge.GaugeType type) {
        _uiController.ToggleWarningGauge(active, type);
    }
    
    private LevelSettings GetLevelSettings() {
        return CurrentLevel < LevelSettings.Count ? LevelSettings[CurrentLevel] : null;
    }
    
    public static GameManager Get() {
        return _instance;
    }

    public static GameSettings GetGameSettings() {
        return _instance.GameSettings;
    }

    public static void LoadNextLevel() {
        CurrentLevel++;
        SceneManager.LoadScene("Game");
    }
}
