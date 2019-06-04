using UnityEngine;
using UnityEngine.UI;

namespace Game {
    
//    [RequireComponent(typeof(AudioSource))]
    public class SoundController : MonoBehaviour {

        [SerializeField] private Slider VolumeSlider;
        
        private AudioSource _source;

        private static SoundController _instance;

        void Awake() {
            _source = GetComponent<AudioSource>();
            
            if (_instance != null)
                Destroy(gameObject);

            _instance = this;
        }
        
        public static SoundController Get() {
            return _instance;
        }

        public void UpdateVolume() {
            float volume = VolumeSlider.value;
            if (volume >= 0 && volume <= 1)
                _source.volume = volume;
        }

        public void PlayClip(AudioClip clip) {
            _source.clip = clip;
            _source.Play();
        }
    }
}
