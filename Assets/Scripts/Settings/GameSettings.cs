using UnityEngine;

namespace Settings {
    
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject {

        public float TimeScale = 1;
        public AudioClip MainTheme;
    }
}