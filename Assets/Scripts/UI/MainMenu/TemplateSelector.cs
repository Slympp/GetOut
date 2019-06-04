using System.Collections;
using System.Collections.Generic;
using Game;
using Settings;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TemplateSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    [SerializeField] private MainMenuController MainMenuController;
    [SerializeField] private CharacterTemplate Template;

    [SerializeField] private TMP_Text TemplateName;
    [SerializeField] private TMP_Text TemplateDescription;
    
    private Button _button;

    void Awake() {
        _button = GetComponent<Button>();

        if (_button == null) {
            Debug.LogError($"TemplateSelector {gameObject.name} => Button not found.");
            return;
        }

        if (Template == null) {
            Debug.LogError($"TemplateSelector {gameObject.name} => Template not found.");
            return;
        }
        
        _button.onClick.AddListener(() => StartGame(Template));
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        TemplateName.text = Template.Name;
        TemplateName.gameObject.SetActive(true);
        
        TemplateDescription.text = Template.Description;
        TemplateDescription.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TemplateName.gameObject.SetActive(false);
        TemplateName.text = "";
        
        TemplateDescription.gameObject.SetActive(false);
        TemplateDescription.text = "";
    }

    private void StartGame(CharacterTemplate template) {
        GameManager.Get().SetTemplate(template);
        GameManager.LoadNextLevel();
    }
}
