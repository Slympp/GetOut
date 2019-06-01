using System;
using System.Collections;
using Level.Activities;
using Settings;
using UnityEngine;
using UnityEngine.AI;

namespace Player {
    public class PlayerController : MonoBehaviour {

        [SerializeField] LayerMask InteractableMask;
        public State CurrentState { get; private set; }

        private GameManager _gameManager;
        private Camera       _camera;
        private NavMeshAgent _agent;

        private Animator _animator;
        private Vector3 worldDeltaPosition = Vector3.zero;
        private Vector3 position = Vector3.zero;
        private int _walkAnimationHash;
    
        void Awake() {
            _gameManager = GameManager.Get();
            if (_gameManager == null)
                Debug.LogError("PlayerController => GameManager not found.");
            
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
            if (_camera == null)
                Debug.LogError("PlayerController => Camera not found.");

            InitAnimator();
        }
        
        private void InitAnimator() {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError("PlayerController => Animator not found.");
            
            _agent.updatePosition = false;
            
            _walkAnimationHash = Animator.StringToHash("walking");
        }

        void Update() {

            if (CurrentState == State.GameOver)
                return;
            
            UpdateTarget();

            UpdatePosition();
        }

        private void UpdateTarget() {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, InteractableMask)) {

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
            }
        }
        
        private void UpdatePosition() {
 
            worldDeltaPosition = _agent.nextPosition - transform.position;

            // Pull agent towards character
            if (worldDeltaPosition.magnitude > _agent.radius)
                _agent.nextPosition = transform.position + 0.9f * worldDeltaPosition;
            _agent.nextPosition = transform.position;
        }
        
        private void OnAnimatorMove() {
            position = _animator.rootPosition;
            position.y = _agent.nextPosition.y;
            transform.root.position = position;
        }

        private IEnumerator MoveTo(Vector3 destination, Action onReach, Vector3 lookAt) {
            _agent.SetDestination(destination);
            _animator.SetBool(_walkAnimationHash, true);
                
            while (_agent.pathPending || 
                   _agent.remainingDistance >= _agent.stoppingDistance || 
                   !_agent.velocity.sqrMagnitude.Equals(0f)) {
                
                yield return new WaitForEndOfFrame();
            }

            _animator.SetBool(_walkAnimationHash, false);
            if (onReach != null) {
                onReach.Invoke();
                
                if (lookAt != Vector3.zero)
                    transform.LookAt(lookAt);
            }
        }

        public void SetState(State state) {
            if (CurrentState != State.GameOver)
                CurrentState = state;
        }
    }

    public enum State {
        Default = 0,
        Busy,
        GameOver
    }
}
