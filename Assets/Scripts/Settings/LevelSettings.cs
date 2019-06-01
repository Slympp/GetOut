using UnityEngine;

namespace Settings {
    
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Settings/Level Settings")]
    public class LevelSettings : ScriptableObject {

        public string Name = "LevelName";
        public float GradesRequirement;
        public int Duration = 300;
        public GameObject LevelPrefab;
    }
}