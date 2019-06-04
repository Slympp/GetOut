using UnityEngine;

namespace Settings {
    [CreateAssetMenu(fileName = "CharacterTemplate", menuName = "CharacterTemplate")]
    public class CharacterTemplate : ScriptableObject {
        public string Name;
        public CharacterType Type;
        public string Description;
        public float GradeMultiplier = 1f;
        public float HappinessMultiplier = 1f;
        public float FatigueMultiplier = 1f;
    }

    // TODO: Conditionals events
    public enum CharacterType {
        A,
        B,
        C
    }
}
