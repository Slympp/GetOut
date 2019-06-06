using System.Collections;
using System.Collections.Generic;
using Level.Activities;
using Player;
using Settings;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    public class GameManager : MonoBehaviour {
    
        public static int CurrentLevel { get; private set; } = -1;
        
        public static CharacterTemplate CharacterTemplate { get; private set; }
        
        public LevelSettings CurrentLevelSettings { get; private set; }
        public SoundController SoundController { get; private set; }
        
        [Header("Levels")]
        [SerializeField] private List<LevelSettings> LevelSettings;

        [Header("Settings")]
        [SerializeField] private GameSettings GameSettings;
    
        [SerializeField] private GaugeSettings GradesSettings;
        public                   Gauge         Grades { get; private set; }
    
        [SerializeField] private GaugeSettings HappinessSettings;
        public                   Gauge         Happiness { get; private set; }
    
        [SerializeField] private GaugeSettings FatigueSettings;
        public                   Gauge         Fatigue { get; private set; }
        
        private GameObject _instantiatedLevel;
        private GameUIController _uiController;
        private PlayerController _playerController;
        
        private const string PlayerPrefabPath = "Prefabs/Characters/Player";
        private static GameManager _instance;
    
        void Awake() {
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(_instance.gameObject);

            SceneManager.sceneLoaded += OnLevelLoaded;
        }

        void OnLevelLoaded(Scene scene, LoadSceneMode mode) {

            SoundController = _instance.gameObject.GetComponent<SoundController>();
            SoundController.SetAudioSources();
            
            if (scene.name != "Game") {
                SoundController.PlayMusic(GameSettings.MainTheme);
                if (_instantiatedLevel != null)
                    Destroy(_instantiatedLevel);
                return;
            }
            
            _uiController = GameObject.Find("GameUI").GetComponent<GameUIController>();
            if (_uiController == null)
                Debug.LogError("GameManager => GameUIController not found");
            
            CurrentLevelSettings = GetLevelSettings();
            if (CurrentLevelSettings == null) {
                Debug.LogError($"GameManager => Settings not found for Level: {CurrentLevel}");  
                return;
            }
            
            SoundController.PlayMusic(CurrentLevelSettings.BackgroundMusic);
            _uiController.InitFonts(CurrentLevelSettings.Font);
            
            InitGauges();
            InitLevel();
            InitPlayer();

            StartCoroutine(nameof(ProgressGame));
            StartCoroutine(_uiController.FadeLevelName(CurrentLevelSettings.Name));
        }

        public void SetTemplate(CharacterTemplate template) {
            CharacterTemplate = template;
        }
        
        public void ApplyModifiers(float grades, float happiness, float fatigue) {
            if (!grades.Equals(0))
                Grades.Value += grades * CharacterTemplate.GradeMultiplier;

            if (!happiness.Equals(0))
                Happiness.Value += happiness * CharacterTemplate.HappinessMultiplier;

            if (!fatigue.Equals(0))
                Fatigue.Value += fatigue * CharacterTemplate.FatigueMultiplier;
        }

        public void ToggleActivityInfos(bool enabled, BaseActivity activity = null) {
            _uiController.ToggleActivityInfo(enabled, activity);
        }

        private void InitGauges() {
            Grades = new Gauge(GradesSettings, UpdateGauge, GameOver, ToggleWarning, CurrentLevelSettings.GradesRequirement);
            _uiController.SetGradeRequirementIndicator(CurrentLevelSettings.GradesRequirement / GradesSettings.MaxValue);
        
            Happiness = new Gauge(HappinessSettings, UpdateGauge, GameOver, ToggleWarning);
            Fatigue = new Gauge(FatigueSettings, UpdateGauge, GameOver, ToggleWarning);
        }

        private void InitLevel() {
            if (_instantiatedLevel != null)
                Destroy(_instantiatedLevel);

            if (CurrentLevelSettings.LevelPrefab != null) {
                _instantiatedLevel = Instantiate(CurrentLevelSettings.LevelPrefab, transform);
                _instantiatedLevel.transform.localPosition = CurrentLevelSettings.LevelPrefab.transform.position;
            } else {
                Debug.LogError($"GameManager => Prefab not found for Level: {CurrentLevelSettings.Name}");  
            }
        }

        private void InitPlayer() {
            GameObject player = Instantiate(Resources.Load<GameObject>(PlayerPrefabPath));
            if (player != null)
                _playerController = player.GetComponentInChildren<PlayerController>();
        }
    
        private IEnumerator ProgressGame() {
            float elapsed = 0;
            float elapsedSinceLastTick = 0;

            _uiController.UpdateGameProgressBar(0);
            
            while (elapsed < CurrentLevelSettings.Duration && _playerController.CurrentState != State.GameOver) {
                elapsed += Time.deltaTime;
                elapsedSinceLastTick += Time.deltaTime;
                if (elapsedSinceLastTick >= GameSettings.TimeScale) {
                    elapsedSinceLastTick = 0;
                    ApplyModifiers(CurrentLevelSettings.GradesReduction, 
                        CurrentLevelSettings.HappinessReduction, 
                        CurrentLevelSettings.FatigueReduction);
                }
            
                _uiController.UpdateGameProgressBar(Mathf.Lerp(0, 1, elapsed / CurrentLevelSettings.Duration));
                yield return new WaitForEndOfFrame();
            }

            if (_playerController.CurrentState != State.GameOver) {
                _uiController.UpdateGameProgressBar(1);
                bool victory = Grades.Value >= CurrentLevelSettings.GradesRequirement;
                GameOver(victory, "Your grades were too low, you failed your final exams");
            }
        }

        private void GameOver(bool victory, string reason) {
            if (_playerController.CurrentState != State.GameOver) {
                _playerController.SetState(State.GameOver);

                if (victory) {
                    _uiController.EnableVictory();
                } else {
                    _uiController.EnableGameOver(reason);
                }
            }
        }
    
        private void UpdateGauge(float value, float max, Gauge.GaugeType type) {
            _uiController.ProgressGauge(value / max, type);
        }

        private void ToggleWarning(bool active, Gauge.GaugeType type, bool isGradeRequirement) {
            _uiController.ToggleWarningGauge(active, type, isGradeRequirement);
        }
    
        private LevelSettings GetLevelSettings() {
            int index = CurrentLevel == -1 ? 0 : CurrentLevel; 
            return index < LevelSettings.Count ? LevelSettings[index] : null;
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
        
        public static void LoadMainMenu() {
            CurrentLevel = -1;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
