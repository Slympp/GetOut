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
    
        void Awake() {
            _gameManager = GameManager.Get();
            if (_gameManager == null)
                Debug.LogError("PlayerController => GameManager not found.");
            
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
            if (_camera == null)
                Debug.LogError("PlayerController => Camera not found.");
        }

        void Update() {

            if (CurrentState == State.GameOver)
                return;
            
            Target();
        }

        void Target() {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, InteractableMask)) {

                if (Input.GetMouseButtonDown(0) && CurrentState != State.Busy) {
                    if (hit.collider.CompareTag("Ground"))
                        StartCoroutine(MoveTo(hit.point, null));

                    if (hit.collider.CompareTag("Activity")) {
                        BaseActivity activity = hit.collider.gameObject.GetComponent<BaseActivity>();
                        if (activity == null) 
                            return;

                        void DoActivity() => StartCoroutine(activity.Do(SetState));
                        StartCoroutine(MoveTo(activity.GetRigPosition(), DoActivity));
                    }
                }
            }
        }

        private IEnumerator MoveTo(Vector3 destination, Action onReach) {
            _agent.SetDestination(destination);
            if (onReach != null) {
                
                while (_agent.pathPending || _agent.remainingDistance >= _agent.stoppingDistance ||
                       _agent.hasPath || !_agent.velocity.sqrMagnitude.Equals(0f)) {
                    yield return new WaitForEndOfFrame();
                }
                transform.LookAt(destination);
                onReach();
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
