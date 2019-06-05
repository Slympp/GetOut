using Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class UISoundController : MonoBehaviour, IPointerEnterHandler {
        [SerializeField] private AudioClip TickSound;
    
        public void OnPointerEnter(PointerEventData eventData) {
            GameManager.Get().SoundController.PlaySound(TickSound);
        }
    }
}
