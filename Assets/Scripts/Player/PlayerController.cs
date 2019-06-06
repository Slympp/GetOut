using System;
using System.Collections;
using Game;
using Level.Activities;
using Settings;
using UnityEngine;
using UnityEngine.AI;

namespace Player {
    public class PlayerController : MonoBehaviour {

        [SerializeField] private LayerMask InteractableMask;
        public State CurrentState { get; private set; }

        private GameManager _gameManager;
        private Camera       _camera;
        private NavMeshAgent _agent;
        private AudioSource _audio;

        private Animator _animator;
        private Vector3 worldDeltaPosition = Vector3.zero;
        private Vector3 position = Vector3.zero;
        private int _walkAnimationHash;
        private int _actionAnimationHash;

        private GameObject _cachedHoveredGameObject;
        private BaseActivity _cachedHoveredActivity;
        private bool _isHovering;
    
        void Awake() {
            _gameManager = GameManager.Get();
            if (_gameManager == null)
                Debug.LogError("PlayerController => GameManager not found.");
            
            _agent = GetComponent<NavMeshAgent>();
            _audio = GetComponent<AudioSource>();
            
            _camera = Camera.main;
            if (_camera == null)
                Debug.LogError("PlayerController => Camera not found.");

            InitAnimator();
        }
        
        private void InitAnimator() {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError("PlayerController => Animator not found.");
            
            _walkAnimationHash = Animator.StringToHash("walking");
            _actionAnimationHash = Animator.StringToHash("doing");
        }

        void Update() {

            if (CurrentState == State.GameOver)
                return;
            
            UpdateTarget();
        }

        private void UpdateTarget() {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, InteractableMask)) {
                
                // Selection
                GameObject target = hit.collider.gameObject;
                if (target.CompareTag("Activity")) {
                    if (_isHovering) {
                      if (_cachedHoveredGameObject != target)
                          SetHovering(target);
                    } else
                        SetHovering(target);
                    
                } else
                    SetHovering(null);
                
                // Interaction (Movement && Activities)
                if (Input.GetMouseButtonDown(0) && CurrentState != State.Busy) {
                    StopAllCoroutines();
                    if (hit.collider.CompareTag("Ground"))
                        StartCoroutine(MoveTo(hit.point, null, Vector3.zero));

                    if (hit.collider.CompareTag("Activity")) {
                        BaseActivity activity = hit.collider.gameObject.GetComponent<BaseActivity>();
                        if (activity == null) 
                            return;

                        void DoActivity() => StartCoroutine(activity.Do(SetState));
                        StartCoroutine(MoveTo(activity.GetRigPosition(), DoActivity, activity.transform.position));
                    }
                }
            } else if (_isHovering)
                SetHovering(null);
        }

        private void SetHovering(GameObject newObject) {
            _isHovering = newObject != null;

            if (_cachedHoveredGameObject != null && _cachedHoveredActivity != null) {
                _cachedHoveredActivity.ToggleHover(false);
                _gameManager.ToggleActivityInfos(false);
            }

            if (_isHovering) {
                _cachedHoveredActivity = newObject.GetComponent<BaseActivity>();
                if (_cachedHoveredActivity != null) {
                    _cachedHoveredActivity.ToggleHover(true);
                    _gameManager.ToggleActivityInfos(true, _cachedHoveredActivity);
                }
            } else
                _cachedHoveredActivity = null;

            _cachedHoveredGameObject = newObject;
        }

        private IEnumerator MoveTo(Vector3 destination, Action onReach, Vector3 lookAt) {
            _agent.SetDestination(destination);
            _animator.SetBool(_walkAnimationHash, true);
            
            _audio.Play();
                
            while (_agent.pathPending || 
                   _agent.remainingDistance >= _agent.stoppingDistance || 
                   !_agent.velocity.sqrMagnitude.Equals(0f)) {
                
                yield return new WaitForEndOfFrame();
            }
            
            _audio.Stop();

            _animator.SetBool(_walkAnimationHash, false);
            if (onReach != null) {
                _animator.SetBool(_actionAnimationHash, true);
                onReach.Invoke();
                
                if (lookAt != Vector3.zero)
                    transform.LookAt(lookAt);
            }
        }

        public void SetState(State state) {
            
            if (CurrentState != State.GameOver) {
                
                if (state != State.Busy)
                    _animator.SetBool(_actionAnimationHash, false);
                
                CurrentState = state;
            }
        }
    }

    public enum State {
        Default = 0,
        Busy,
        GameOver
    }
}
