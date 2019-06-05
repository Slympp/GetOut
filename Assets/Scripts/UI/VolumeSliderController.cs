using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class VolumeSliderController : MonoBehaviour {

        [SerializeField] private GameObject Root;
        void Start() {
            GameManager.Get().SoundController.InitSoundController(GetComponent<Slider>());
            Root.SetActive(false);
        }
    }
}
