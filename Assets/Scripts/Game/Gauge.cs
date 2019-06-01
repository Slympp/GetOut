using System;
using Settings;

namespace Game {
    public class Gauge {
        
        public enum GaugeType {
            Grades = 0,
            Happiness,
            Fatigue
        }
        
        public float Value {
            get => _value;
            set {
                if (!_value.Equals(value)) {
                    _value = value;
                    
                    // Max boundary
                    if (_value > _maxValue)
                        _value = _maxValue;

                    // Toggle Warning
                    if (_value <= _warningValue && !_wasWarning) {
                        _wasWarning = true;
                        OnWarningCallback();
                    } else if (_value > _warningValue && _wasWarning) {
                        _wasWarning = false;
                        OnWarningCallback();
                    }
                    
                    // Min boundary
                    if (_value <= 0) {
                        _value = 0;
                        _onReachZero?.Invoke(false, _gameOverReason);
                    }

                    OnUpdateCallback();
                }
            }
        }

        private readonly Action<float, float, GaugeType> _onUpdate;
        private readonly GaugeType _type;
        private          float     _value;
        private readonly float     _maxValue;

        private Action<bool, GaugeType> _onWarningUpdate;
        private readonly float _warningValue;
        private bool _wasWarning;
            
        private readonly Action<bool, string> _onReachZero;
        private readonly string _gameOverReason;

        public Gauge(GaugeSettings settings, Action<float, float, GaugeType> onUpdate, 
            Action<bool, string> onReachZero, Action<bool, GaugeType> onWarningUpdate) {
            
            _onUpdate = onUpdate;
            _onReachZero = onReachZero;
            _onWarningUpdate = onWarningUpdate;
            
            _maxValue = settings.MaxValue;
            _warningValue = settings.WarningValue;
            _type = settings.Type;
            _value = settings.DefaultValue;
            _gameOverReason = settings.GameOverReason;
            
            _wasWarning = false;
            
            // Trigger callback at init
            OnUpdateCallback();
        }

        private void OnUpdateCallback() {
            _onUpdate?.Invoke(_value, _maxValue, _type);
        }

        private void OnWarningCallback() {
            _onWarningUpdate?.Invoke(_wasWarning, _type);
        }
    }
}