using System;
using Settings;
using UnityEngine;

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
                    if (_value <= _warningValue) {
                        if (!_wasWarning) {
                            _wasWarning = true;
                            _wasRequirement = false;
                            OnWarningCallback();
                        }
                    } else if (!_requirement.Equals(0) && _value <= _requirement) {
                        if (!_wasRequirement) {
                            _wasWarning = false;
                            _wasRequirement = true;
                            OnWarningCallback();
                        }
                    } else if ((_wasWarning  && _value > _warningValue) || 
                               (!_requirement.Equals(0) && _wasRequirement && _value > _requirement)) {
                        _wasWarning = false;
                        _wasRequirement = false;
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

        private float _value;
        private readonly Action<float, float, GaugeType> _onUpdate;
        private readonly GaugeType _type;
        private readonly float     _maxValue;
        private readonly float _requirement;

        private readonly Action<bool, GaugeType, bool> _onWarningUpdate;
        private readonly float _warningValue;
        private bool _wasWarning;
        private bool _wasRequirement;
            
        private readonly Action<bool, string> _onReachZero;
        private readonly string _gameOverReason;

        public Gauge(GaugeSettings settings, Action<float, float, GaugeType> onUpdate, 
            Action<bool, string> onReachZero, Action<bool, GaugeType, bool> onWarningUpdate, float requirement = 0) {
            
            _onUpdate = onUpdate;
            _onReachZero = onReachZero;
            _onWarningUpdate = onWarningUpdate;
            
            _maxValue = settings.MaxValue;
            _warningValue = settings.WarningValue;
            _requirement = requirement;
            _type = settings.Type;
            _value = settings.DefaultValue;
            _gameOverReason = settings.GameOverReason;
            
            _wasWarning = false;
            _wasRequirement = false;
            
            // Trigger callback at init
            OnUpdateCallback();
        }

        private void OnUpdateCallback() {
            _onUpdate?.Invoke(_value, _maxValue, _type);
        }

        private void OnWarningCallback() {
            _onWarningUpdate?.Invoke(_wasWarning, _type, _wasRequirement);
        }
    }
}