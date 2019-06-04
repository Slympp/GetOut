using TMPro;
using UnityEngine;

namespace Settings {
    
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Settings/Level Settings")]
    public class LevelSettings : ScriptableObject {

        public string Name = "LevelName";
        public TMP_FontAsset Font;
        public float GradesRequirement;
        public int Duration = 300;
        public float GradesReduction;
        public float HappinessReduction;
        public float FatigueReduction;
        public GameObject LevelPrefab;
        public AudioClip BackgroundMusic;
        public bool LastLevel = false;
    }
}