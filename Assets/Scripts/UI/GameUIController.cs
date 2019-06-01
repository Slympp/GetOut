using System.Collections;
using Game;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GameUIController : MonoBehaviour {

        [SerializeField] private UISettings Settings;

        [SerializeField] private Image GameProgress;
        
        [SerializeField] private Image GradeValue;
        [SerializeField] private Image GradeWarning;
        [SerializeField] private Transform GradeRequirement;
        
        [SerializeField] private Image HappinessValue;
        [SerializeField] private Image HappinessWarning;

        [SerializeField] private Image FatigueValue;
        [SerializeField] private Image FatigueWarning;

        [SerializeField] private GameObject GameOver;
        [SerializeField] private TMP_Text GameOverReason;

        public void EnableGameOver(string reason) {
            GameOver.SetActive(true);
            GameOverReason.text = reason;
        }
        
        public void UpdateGameProgressBar(float value) {
            GameProgress.fillAmount = value;
        }

        public void SetGradeRequirementIndicator(float value) {
            GradeRequirement.localPosition = new Vector3(Mathf.Clamp(value, 2, 198), 0, 0);
        }
        
        public void ProgressGauge(float newPercent, Gauge.GaugeType type) {
            StartCoroutine(ProgressBar(new UpdateData(GetImageByGaugeType(type), newPercent)));
        }

        public void ToggleWarningGauge(bool active, Gauge.GaugeType type) {
            GetImageByGaugeType(type, false).gameObject.SetActive(active);
        }

        private Image GetImageByGaugeType(Gauge.GaugeType type, bool value = true) {
            switch (type) {
                case Gauge.GaugeType.Grades:
                    return value ? GradeValue : GradeWarning;
                case Gauge.GaugeType.Happiness:
                    return value ? HappinessValue : HappinessWarning;
                case Gauge.GaugeType.Fatigue:
                    return value ? FatigueValue : FatigueWarning;
            }
            return null;
        }

        private IEnumerator ProgressBar(UpdateData d) {
            if (d.Image == null)
                yield return null;
            
            float originPercent = d.Image.fillAmount;
            float elapsed = 0;

            while (elapsed < Settings.BarProgressSpeed) {
                elapsed += Time.deltaTime;
                d.Image.fillAmount = Mathf.Lerp(originPercent, d.TargetPercent, elapsed / Settings.BarProgressSpeed);
                yield return new WaitForEndOfFrame();
            }
            d.Image.fillAmount = d.TargetPercent;
        }

        private struct UpdateData {
            public readonly Image Image;
            public readonly float TargetPercent;
            public UpdateData(Image image, float targetPercent) {
                Image = image;
                TargetPercent = targetPercent;
            }
        }
    }
}
