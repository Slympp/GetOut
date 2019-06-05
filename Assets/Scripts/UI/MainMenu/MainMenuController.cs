using System.Runtime.InteropServices;
using UnityEngine;

namespace UI {
    public class MainMenuController : MonoBehaviour {

        [SerializeField] private GameObject MenuUI;
        [SerializeField] private GameObject CharacterSelectionUI;
        [SerializeField] private GameObject SettingsUI;
        [SerializeField] private GameObject CreditsUI;

//        [DllImport("__Internal")]
//        private static extern void openWindow(string url);
     
        public void OpenURL(string url) {
            if (!string.IsNullOrEmpty(url)) {
//                #if !UNITY_EDITOR
//                    openWindow(url);
//                #else
                    Application.OpenURL(url);
//                #endif
            }
        }
        
        public void ToggleCharacterSelectionUI() {
            ToggleMainUI();
            CharacterSelectionUI.SetActive(!CharacterSelectionUI.activeSelf);
        }
        
        public void ToggleSettingsUI() {
            ToggleMainUI();
            SettingsUI.SetActive(!SettingsUI.activeSelf);
        }
        
        public void ToggleCreditsUI() {
            ToggleMainUI();
            CreditsUI.SetActive(!CreditsUI.activeSelf);
        }

        private void ToggleMainUI() {
            MenuUI.SetActive(!MenuUI.activeSelf);
        }
    }
}
