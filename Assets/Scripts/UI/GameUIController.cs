using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Level.Activities;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI {
    public class GameUIController : MonoBehaviour {

        [Header("Main")]
        [SerializeField] private UISettings Settings;
        [SerializeField] private TMP_Text LevelName;
        [SerializeField] private float FadeOutTime = 1.5f;

        [Header("Top UI")]
        [SerializeField] private GameObject TopUI;
        [SerializeField] private Image GameProgress;
        [SerializeField] private RectTransform GradeRequirement;
        [SerializeField] private Image GradeValue;
        [SerializeField] private Image GradeWarning;
        [SerializeField] private Image HappinessValue;
        [SerializeField] private Image HappinessWarning;
        [SerializeField] private Image FatigueValue;
        [SerializeField] private Image FatigueWarning;
        [SerializeField] private Color WarningColor;
        [SerializeField] private Color RequirementColor;
        
        [Header("GameOver")]
        [SerializeField] private GameObject GameOverUI; 
        [SerializeField] private TMP_Text GameOverSubtitle;
        
        [Header("Victory")]
        [SerializeField] private GameObject VictorUI;
        [SerializeField] private TMP_Text VictorySubtitle;
        
        [Header("EndGame")]
        [SerializeField] private GameObject EndGameUI;
        
        [Header("ActivityInfosUI")]
        [SerializeField] private GameObject ActivityInfoUI;
        [SerializeField] private TMP_Text ActivityName;
        [SerializeField] private Image ActivityGradeModifierImage;
        [SerializeField] private Image ActivityHappinessModifierImage;
        [SerializeField] private Image ActivityFatigueModifierImage;
        [SerializeField] private Color DefaultColor;
        [SerializeField] private Color PositiveColor;
        [SerializeField] private Color NegativeColor;

        [Header("Settings UI")] 
        [SerializeField] private GameObject SettingsUI;

        
        public void InitFonts(TMP_FontAsset font) {
            if (font == null) {
                Debug.LogError("GameUIController => Font not found for this level.");
                return;
            }

            ActivityName.font = font;
            LevelName.font = font;

            TMP_Text[] gameOverTexts = GameOverUI.GetComponentsInChildren<TMP_Text>();
            foreach (var text in gameOverTexts)
                text.font = font;
            
            TMP_Text[] victoryTexts = VictorUI.GetComponentsInChildren<TMP_Text>();
            foreach (var text in victoryTexts)
                text.font = font;
        }

        public void EnableVictory() {
            ToggleActivityInfo(false, null);
            TopUI.SetActive(false);

            if (!GameManager.Get().CurrentLevelSettings.LastLevel) {
                VictorUI.SetActive(true);
                VictorySubtitle.text = $"You passed {GameManager.Get().CurrentLevelSettings.Name} final tests";
            } else {
                EndGameUI.SetActive(true);
            }
        }
        
        public void EnableGameOver(string reason) {
            ToggleActivityInfo(false, null);
            TopUI.SetActive(false);
            GameOverUI.SetActive(true);
            GameOverSubtitle.text = reason;
        }

        public void ToggleActivityInfo(bool enabled, BaseActivity activity) {
            if (enabled && activity != null) {
                ActivityName.text = activity.GetName();
                SetGaugeModifierImageColor(ActivityGradeModifierImage, activity.GetGradeModifier());
                SetGaugeModifierImageColor(ActivityHappinessModifierImage, activity.GetHappinessModifier());
                SetGaugeModifierImageColor(ActivityFatigueModifierImage, activity.GetFatigueModifier());
            }
            ActivityInfoUI.SetActive(enabled);
        }
        
        private void SetGaugeModifierImageColor(Image image, float modifier) {
            image.color = modifier.Equals(0) ? DefaultColor
                : modifier > 0 ? PositiveColor : NegativeColor;
        }

        public void ToggleSettings() {
            SettingsUI.SetActive(!SettingsUI.activeSelf);
        }

        public IEnumerator FadeLevelName(string _name) {
            LevelName.text = _name;
            Color originalColor = LevelName.color;
            
            for (float t = 0.01f; t < FadeOutTime; t += Time.deltaTime) {
                LevelName.color = Color.Lerp(originalColor, Color.clear, Mathf.Min(1, t/FadeOutTime));
                yield return new WaitForEndOfFrame();
            }
            LevelName.color = Color.clear;
            yield return new WaitForSeconds(1.5f);
            LevelName.gameObject.SetActive(false);
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
            float xOffset = value * 196 + 2 - 100;
            Vector3 position = new Vector3(xOffset, 0, 0);
            GradeRequirement.localPosition = position;
        }
        
        public void ProgressGauge(float newPercent, Gauge.GaugeType type) {
            StartCoroutine(ProgressBar(new UpdateProgressData(GetImageByGaugeType(type), newPercent)));
        }

        public void ToggleWarningGauge(bool active, Gauge.GaugeType type, bool isGradeRequirement) {
            Image image = GetImageByGaugeType(type, false);

            if (image != null) {
                image.color = isGradeRequirement ? RequirementColor : WarningColor;
                image.gameObject.SetActive(active || isGradeRequirement);
            }
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

        private IEnumerator ProgressBar(UpdateProgressData d) {
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

        private struct UpdateProgressData {
            public readonly Image Image;
            public readonly float TargetPercent;
            public UpdateProgressData(Image image, float targetPercent) {
                Image = image;
                TargetPercent = targetPercent;
            }
        }
    }
}
