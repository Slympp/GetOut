using UnityEngine;

namespace Settings {
    
    [CreateAssetMenu(fileName = "UISettings", menuName = "Settings/UI Settings")]
    public class UISettings : ScriptableObject {

        public float BarProgressSpeed = 0.2f;
    }
}