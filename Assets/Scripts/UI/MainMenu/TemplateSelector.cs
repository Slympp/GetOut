using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Settings;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TemplateSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    [SerializeField] private CharacterTemplate Template;

    [SerializeField] private TMP_Text TemplateName;
    [SerializeField] private TMP_Text TemplateDescription;

    private const string DefaultText = "Select a Character";
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
        TemplateName.text = DefaultText;
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        TemplateName.text = Template.Name;
        
        TemplateDescription.text = Template.Description;
        TemplateDescription.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TemplateName.text = DefaultText;
        
        TemplateDescription.gameObject.SetActive(false);
        TemplateDescription.text = "";
    }

    private void StartGame(CharacterTemplate template) {
        GameManager.Get().SetTemplate(template);
        GameManager.LoadNextLevel();
    }
}
