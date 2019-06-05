using UnityEngine;

namespace Level.Activities {
    public class SpinningActivity : BaseActivity {

        [SerializeField] private float rotationSpeed = 1f;
    
        private bool spinning;
    
        protected override void OnStart() {
            spinning = true;
        }

        protected override void OnEnd() {
            spinning = false;
        }

        void FixedUpdate() {
            if (!spinning)
                return;
        
            transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        }
    }
}
