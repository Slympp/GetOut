using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    
    public class SoundController : MonoBehaviour {

        private AudioSource[] _sources;
//        private AudioSource _soundsSource;
        private Slider _volumeSlider;

        private float _volumeValue = 0.5f;

        public void SetAudioSources() {
            _sources = GetComponents<AudioSource>();
        }

        public void InitSoundController(Slider slider) {
            // MUSIC
            _sources[0].playOnAwake = true;
            _sources[0].loop = true;
            
            // SOUNDS
            _sources[1].playOnAwake = false;
            _sources[1].loop = false;
            
            _volumeSlider = slider;
            _volumeSlider.onValueChanged.RemoveAllListeners();
            _volumeSlider.value = _volumeValue;
            _volumeSlider.onValueChanged.AddListener(UpdateVolume);
            UpdateVolume(_volumeValue);
        }
        
        public void UpdateVolume(float value) {
            _volumeValue = value;
            if (!(_volumeValue >= 0) || !(_volumeValue <= 1)) return;
            
            foreach (var s in _sources) {
                s.volume = _volumeValue;
            }
        }

        public void PlayMusic(AudioClip clip) {
            _sources[0].clip = clip;
            _sources[0].Play();
        }

        public void PlaySound(AudioClip clip) {
            if (_sources[1] == null)
                SetAudioSources();
            
            _sources[1].clip = clip;
            _sources[1].Play();
        }
    }
}
