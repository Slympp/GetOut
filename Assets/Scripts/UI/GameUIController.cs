using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI {
    public class GameUIController : MonoBehaviour {

        [SerializeField] private UISettings Settings;

        [SerializeField] private GameObject TopUI;
        [SerializeField] private GameObject VictorUI;
        [SerializeField] private TMP_Text VictorySubtitle;
        [SerializeField] private GameObject GameOverUI; 
        [SerializeField] private TMP_Text GameOverSubtitle;
        
        [SerializeField] private Image GameProgress;
        
        [SerializeField] private Image GradeValue;
        [SerializeField] private Image GradeWarning;
        [SerializeField] private Transform GradeRequirement;
        
        [SerializeField] private Image HappinessValue;
        [SerializeField] private Image HappinessWarning;

        [SerializeField] private Image FatigueValue;
        [SerializeField] private Image FatigueWarning;

        public void InitFonts(TMP_FontAsset font) {
            if (font == null) {
                Debug.LogError("GameUIController => Font not found for this level.");
                return;
            }

            TMP_Text[] gameOverTexts = GameOverUI.GetComponentsInChildren<TMP_Text>();
            foreach (var text in gameOverTexts)
                text.font = font;
            
            TMP_Text[] victoryTexts = VictorUI.GetComponentsInChildren<TMP_Text>();
            foreach (var text in victoryTexts)
                text.font = font;
        }

        public void EnableVictory() {
            TopUI.SetActive(false);
            VictorUI.SetActive(true);
            VictorySubtitle.text = $"You passed {GameManager.Get().CurrentLevelSettings.Name} final tests";
        }
        
        public void EnableGameOver(string reason) {
            TopUI.SetActive(false);
            GameOverUI.SetActive(true);
            GameOverSubtitle.text = reason;
        }

        public void LoadMainMenu() {
            GameManager.LoadMainMenu();
        }

        public void LoadNextLevel() {
            if (GameManager.Get().CurrentLevelSettings.LastLevel) {
                GameManager.LoadMainMenu();
            } else {
                GameManager.LoadNextLevel();
            }
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
