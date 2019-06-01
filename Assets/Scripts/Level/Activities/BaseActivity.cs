using System;
using System.Collections;
using Player;
using Settings;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Level.Activities {
    public abstract class BaseActivity : MonoBehaviour {

        [SerializeField] private Transform RigPosition;
        
        [SerializeField] private string Name;
        [SerializeField] private string Description;
        
        [SerializeField] private float GradeModifier;
        [SerializeField] private float HappinessModifier;
        [SerializeField] private float FatigueModifier;
        [SerializeField] private float Duration = 3f;
        
        [SerializeField] private Material DefaultMaterial;
        [SerializeField] private Material HighlightedMaterial;

        private const string TimerPath = "Prefabs/Activities/Timer";
        private GameObject _timerObject;
        private Image _timer;

        private MeshRenderer _meshRenderer;
        private GameManager _gameManager;
        private GameSettings _settings;

        protected void Awake() {
            _gameManager = GameManager.Get();
            _settings = GameManager.GetSettings();

            _timerObject = Instantiate(Resources.Load<GameObject>(TimerPath), transform);
            if (_timerObject) {
                _timer = _timerObject.GetComponent<TimerController>().ProgressImage;
            } else {
                Debug.LogError($"Activity {gameObject.name} failed to instantiate Timer");
            }
            
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (_meshRenderer != null) {
                ToggleActive(false);
            } else {
                Debug.LogError($"MeshRenderer not found in activity {gameObject.name} children.");
            }
        }

        public void ToggleActive(bool active) {
            _meshRenderer.material = active ? HighlightedMaterial : DefaultMaterial;
            
            _timer.fillAmount = active ? 1 : 0;
            _timerObject.SetActive(active);
        }
        
        public Vector3 GetRigPosition() {
            return RigPosition != null ? RigPosition.position : transform.position;
        }

        public IEnumerator Do(Action<State> setState) {

            setState(State.Busy);
            ToggleActive(true);
            // Play animation on Activity

            
            float timeElapsed = 0;
            float timeSinceLastTick = 0;
            while (timeElapsed < Duration * _settings.TimeScale) {

                _timer.fillAmount = Mathf.Lerp(1, 0, timeElapsed /(Duration * _settings.TimeScale));

                timeElapsed += Time.deltaTime;
                timeSinceLastTick += Time.deltaTime;
                if (timeSinceLastTick >= _settings.TimeScale) {
                    timeSinceLastTick = 0;
                    ApplyModifiers();
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            ApplyModifiers();
            
            // Stop animation on Activity
            ToggleActive(false);
            setState(State.Default);
        }

        private void ApplyModifiers() {
            if (!GradeModifier.Equals(0)) {
                _gameManager.Grades.Value += GradeModifier;
            }
            
            if (!HappinessModifier.Equals(0)) {
                _gameManager.Happiness.Value += HappinessModifier;
            }
            
            if (!FatigueModifier.Equals(0)) {
                _gameManager.Fatigue.Value += FatigueModifier;
            }
        }
    }
}
