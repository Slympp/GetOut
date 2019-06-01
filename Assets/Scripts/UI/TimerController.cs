using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class TimerController : MonoBehaviour {
        public Image ProgressImage;

        private Transform _camera;
        private bool _isCameraNotNull;

        void Start() {
            if (Camera.main != null) _camera = Camera.main.transform;
            _isCameraNotNull = _camera != null;
        }
        
        void Update() {
            if (_isCameraNotNull) {
                transform.LookAt(transform.position + _camera.transform.rotation * Vector3.back, _camera.rotation * Vector3.up);
            }
        }
    }
}