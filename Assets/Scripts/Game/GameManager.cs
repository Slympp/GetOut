using System.Collections;
using System.Collections.Generic;
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
        
        private static bool MainMenu = true;
        private static GameManager _instance;
    
        void Awake() {
        
            if (_instance != null)
                Destroy(gameObject);

            _instance = this;

            if (MainMenu)
                return;

            _uiController = GameObject.Find("GameUI").GetComponent<GameUIController>();
            if (_uiController == null)
                Debug.LogError("GameManager => GameUIController not found");
        }

        void Start() {

            if (MainMenu)
                return;
            

            CurrentLevelSettings = GetLevelSettings();
            if (CurrentLevelSettings == null) {
                Debug.LogError($"GameManager => Settings not found for Level: {CurrentLevel}");  
                return;
            }
            
            SoundController.Get().PlayClip(CurrentLevelSettings.BackgroundMusic);
            _uiController.InitFonts(CurrentLevelSettings.Font);
            InitGauges(CurrentLevelSettings);
            InitLevel(CurrentLevelSettings);
            InitPlayer();

            StartCoroutine(ProgressGame(CurrentLevelSettings.Duration));
        }

        public void SetTemplate(CharacterTemplate template) {
            CharacterTemplate = template;
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
            GameObject player = Instantiate(Resources.Load<GameObject>(PlayerPrefabPath));
            if (player != null)
                _playerController = player.GetComponentInChildren<PlayerController>();
        }
    
        private IEnumerator ProgressGame(float duration) {
            float elapsed = 0;
            float elapsedSinceLastTick = 0;

            _uiController.UpdateGameProgressBar(0);
            
            while (elapsed < duration && _playerController.CurrentState != State.GameOver) {
                elapsed += Time.deltaTime;
                elapsedSinceLastTick += Time.deltaTime;
                if (elapsedSinceLastTick >= GameSettings.TimeScale) {
                    elapsedSinceLastTick = 0;
                    ApplyModifiers(CurrentLevelSettings.GradesReduction, 
                        CurrentLevelSettings.HappinessReduction, 
                        CurrentLevelSettings.FatigueReduction);
                }
            
                _uiController.UpdateGameProgressBar(Mathf.Lerp(0, 1, elapsed / duration));
                yield return new WaitForEndOfFrame();
            }

            if (_playerController.CurrentState != State.GameOver) {
                _uiController.UpdateGameProgressBar(1);
                bool victory = Grades.Value >= CurrentLevelSettings.GradesRequirement;
                GameOver(victory, "Your grades were too low, you failed your final exams");
            }
        }

        public void ApplyModifiers(float grades, float happiness, float fatigue) {
            if (!grades.Equals(0))
                Grades.Value += grades * CharacterTemplate.GradeMultiplier;

            if (!happiness.Equals(0))
                Happiness.Value += happiness * CharacterTemplate.HappinessMultiplier;

            if (!fatigue.Equals(0))
                Fatigue.Value += fatigue * CharacterTemplate.FatigueMultiplier;
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

        private void ToggleWarning(bool active, Gauge.GaugeType type) {
            _uiController.ToggleWarningGauge(active, type);
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
            MainMenu = false;
            CurrentLevel++;
            SceneManager.LoadScene("Game");
        }
        
        public static void LoadMainMenu() {
            MainMenu = true;
            CurrentLevel = -1;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
