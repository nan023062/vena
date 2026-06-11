// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    [RequireComponent(typeof(RectTransform))]
    public class UIScaleButton : UICustomEvent
    {
        private enum State
        {
            Normal,
            Press,
            Release,
        }

        private State _state = State.Normal;

        protected override void Awake()
        {
            base.Awake();
            _defaultScale = rectTransform.localScale;
            _currentScale = 1f;
        }

        public override void OnPressed(Vector2 position)
        {
            if (_disableEvent) return;
            _pressPoint = position;
            OnPress();
        }

        public override void OnPressOther(GameObject go)
        {
            if (_disableEvent) return;
            onPressOther?.Invoke(go);
        }

        public override void OnRelease(Vector2 position)
        {
            if (_state == State.Press) _state = State.Release;
        }

        public override void OnDragging(Vector2 position, Vector2 delta)
        {
            if (_disableEvent) return;
            ProcessOnDragging(position, delta);
        }

        public override void OnBeginDrag(Vector2 position, Vector2 delta)
        {
            if (_disableEvent) return;
            //目前处理 拖拽不触发点击事件
            _state = State.Release;
            _lastPressTime = MaxTimeValue;
            ProcessBeginDrag(position, delta);
        }

        public override void OnEndDrag(Vector2 position)
        {
            //目前处理 拖拽不触发点击事件
            _state = State.Release;
            _lastPressTime = MaxTimeValue;
            ProcessEndDrag(position);
        }

        private void Update()
        {
            UpdateScaling();
        }

        #region Tween Scaling 

        public bool disableScale = false;

        private float pressScale = 0.9f;

        [Range(0.5f, 2.0f)]
        public float scaleSpeed = 1.2f;

        private float _currentScale = 1f;

        private Vector3 _defaultScale = Vector3.one;

        private void UpdateScaling()
        {
            if (_state == State.Press)
            {
                if (_currentScale <= pressScale)
                {
                    CheckLongPress();
                    return;
                }
                _currentScale *= 1 - Time.unscaledDeltaTime * scaleSpeed;
                _currentScale = Mathf.Clamp(_currentScale, pressScale, 1f);
                if(!disableScale) rectTransform.localScale = _defaultScale * _currentScale;
            }
            else if (_state == State.Release)
            {
                _currentScale *= 1 + Time.unscaledDeltaTime * scaleSpeed;
                _currentScale = Mathf.Clamp(_currentScale, pressScale, 1f);
                if (!disableScale) rectTransform.localScale = _defaultScale * _currentScale;
                if (_currentScale >= 1f) OnReleaseEnded();
            }
        }

        public void SetDefaultScale(float scale)
        {
            _defaultScale = scale * Vector3.one;
        }

        public void SetDisableScale(bool enable)
        {
            disableScale = enable;
        }

        #endregion

        #region Use As Button

        private const float MaxTimeValue = float.MaxValue - 1000f;
        const float ClickThreshold = 0.5f;
        const float LongPressThreshold = 1.2f;
        const float DoubleThreshold = 0.3f;
        private Vector2 _pressPoint = Vector2.zero;
        
        public event VoidDelegate onClick;            //点击事件
        public event VoidDelegate onLongPress;        //长按事件
        public event VoidDelegate onPress;
        public event VoidDelegate onPressOther;
        public event VoidDelegate onDoubleClick;      //双击事件
        
        private float _lastClickTime = 0f;
        private float _lastPressTime = 0f;

        private void OnPress()
        {
            if (_lastPressTime < MaxTimeValue &&
                Time.unscaledTime <= _lastPressTime + DoubleThreshold)
            {
                onDoubleClick?.Invoke(go);
                _lastPressTime = MaxTimeValue;
            }
            else
            {
                _state = State.Press;
                onPress?.Invoke(go);
                _lastPressTime = Time.unscaledTime;
            }
        }

        private void OnReleaseEnded()
        {
            _state = State.Normal;
            if (Time.unscaledTime > _lastClickTime + ClickThreshold
                && Time.unscaledTime > _lastPressTime)
            {
                _lastClickTime = Time.unscaledTime;
                OnClick();
                onClick?.Invoke(go);
            }
        }

        private void CheckLongPress()
        {
            if (Time.unscaledTime > _lastPressTime + LongPressThreshold)
            {
                OnLongPress();
                onLongPress?.Invoke(go);
                _lastPressTime = MaxTimeValue;
            }
        }

        protected virtual void OnDoubleClick()
        {
            
        }

        protected virtual void OnClick()
        {

        }

        protected virtual void OnLongPress()
        {

        }

        #endregion

        #region Drag Event
        
        public event Action<Vector2, Vector2> onBeginDrag;
        public event Action<Vector2, Vector2> onDragging;
        public event Action<Vector2> onEndDrag;
        
        private void ProcessBeginDrag(Vector2 position, Vector2 delta)
        {
            onBeginDrag?.Invoke(position, delta);
        }

        private void ProcessOnDragging(Vector2 position, Vector2 delta)
        {
            onDragging?.Invoke(position, delta);
        }

        private void ProcessEndDrag(Vector2 position)
        {
            onEndDrag?.Invoke(position);
        }

        #endregion
        
        #region Renderer Methods

        private MaskableGraphic[] _graphics;

        private Material _disableMaterial;

        private bool _disable = false;
        
        private bool _disableEvent = false;

        public void SetDisable(bool disable)
        {
            if (_disable != disable)
            {
                _disable = disable;
                _graphics ??= GetComponentsInChildren<MaskableGraphic>(true);
                
                if (_disableMaterial == null)
                {
                    _disableMaterial = new Material(Shader.Find("Game/UI/Gray"));
                }

                if (_graphics == null) return;
                
                foreach (var graphic in _graphics)
                {
                    graphic.material = disable ? _disableMaterial : null;
                }
            }
        }

        public void SetDisableEvent(bool disableEvent)
        {
            _disableEvent = disableEvent;
        }

        #endregion
    }
}
