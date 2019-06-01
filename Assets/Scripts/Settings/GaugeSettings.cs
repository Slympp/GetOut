using Game;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "GaugeSettings", menuName = "Settings/Gauge Settings")]
    public class GaugeSettings : ScriptableObject {
        public Gauge.GaugeType Type;
        public float DefaultValue = 50;
        public float WarningValue = 25;
        public float MaxValue = 100;
        public string GameOverReason = "You got [reason]";
    }
}