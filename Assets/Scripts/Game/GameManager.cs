using Game;
using Player;
using Settings;
using UI;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private static GameManager _instance;

    [SerializeField] private GameSettings GameSettings;
    
    [SerializeField] private GaugeSettings GradesSettings;
    public Gauge Grades    { get; private set; }
    
    [SerializeField] private GaugeSettings HappinessSettings;
    public Gauge Happiness { get; private set; }
    
    [SerializeField] private GaugeSettings FatigueSettings;
    public Gauge Fatigue   { get; private set; }

    private GameUIController _uiController;

    private const string PlayerPath = "Prefabs/Player";
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
        InitPlayer();
    }

    private void InitGauges() {
        Grades = new Gauge(GradesSettings, UpdateGauge, GameOver, ToggleWarning);
        Happiness = new Gauge(HappinessSettings, UpdateGauge, GameOver, ToggleWarning);
        Fatigue = new Gauge(FatigueSettings, UpdateGauge, GameOver, ToggleWarning);
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
}
