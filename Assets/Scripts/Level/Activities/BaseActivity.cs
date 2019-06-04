﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Player;
using Settings;
using UI;
using UnityEngine;
using UnityEngine.Playables;
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
        
        private const string CursorPath = "Prefabs/Models/Cursor";
        private const float CursorYOffset = 2f;
        private GameObject _cursorObject;

        private List<MeshRenderer> _meshRenderers;
        private PlayableDirector _playableDirector;
        
        private GameManager _gameManager;
        private GameSettings _settings;

        protected void Awake() {
            InstantiateElements();
            
            _meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
            _playableDirector = GetComponentInChildren<PlayableDirector>();
            
            if (_meshRenderers != null) {
                ToggleActive(false);
            } else {
                Debug.LogError($"MeshRenderer not found in activity {gameObject.name} children.");
            }
        }

        protected void InstantiateElements() {
            _timerObject = Instantiate(Resources.Load<GameObject>(TimerPath), transform);
            if (_timerObject) {
                _timer = _timerObject.GetComponent<TimerController>().ProgressImage;
            } else {
                Debug.LogError($"Activity {gameObject.name} failed to instantiate Timer");
            }
            
            _cursorObject = Instantiate(Resources.Load<GameObject>(CursorPath), transform);
            _cursorObject.transform.localPosition = new Vector3(0, CursorYOffset, 0);
            if (_cursorObject) {
                ToggleHover(false);
            } else {
                Debug.LogError($"Activity {gameObject.name} failed to instantiate Cursor");
            }
        }

        protected void Start() {
            _gameManager = GameManager.Get();
            _settings = GameManager.GetGameSettings();
        }

        public void ToggleHover(bool active) {
            _cursorObject.SetActive(active);
        }

        public void ToggleActive(bool active) {
            foreach (MeshRenderer meshRenderer in _meshRenderers) {
                meshRenderer.material = active ? HighlightedMaterial : DefaultMaterial;
            }
            
            _timer.fillAmount = active ? 1 : 0;
            _timerObject.SetActive(active);

            if (_playableDirector) {
                if (active)
                    _playableDirector.Play();
                else
                    _playableDirector.Stop();
            }
        }
        
        public Vector3 GetRigPosition() {
            return RigPosition != null ? RigPosition.position : transform.position;
        }

        public IEnumerator Do(Action<State> setState) {

            setState(State.Busy);
            ToggleActive(true);
            // Play animation on Activity

            
            float elapsed = 0;
            float elapsedSinceLastTick = 0;
            while (elapsed < Duration * _settings.TimeScale) {

                _timer.fillAmount = Mathf.Lerp(1, 0, elapsed /(Duration * _settings.TimeScale));

                elapsed += Time.deltaTime;
                elapsedSinceLastTick += Time.deltaTime;
                if (elapsedSinceLastTick >= _settings.TimeScale) {
                    elapsedSinceLastTick = 0;
                    _gameManager.ApplyModifiers(GradeModifier, HappinessModifier, FatigueModifier);
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            _gameManager.ApplyModifiers(GradeModifier, HappinessModifier, FatigueModifier);
            
            // Stop animation on Activity
            ToggleActive(false);
            setState(State.Default);
        }
    }
}
