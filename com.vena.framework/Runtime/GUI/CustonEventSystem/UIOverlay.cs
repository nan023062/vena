// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    public class UIOverlay : UIScaleButton
    {
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _graphicRaycaster;
        private Canvas _canvas;
        private Canvas _parent;
        private bool _isOverlay;
        private UIScaleButton[] _childUIObjs;
        
        public event Action<bool> OnOverlay;
        
        protected override void Awake()
        {
            base.Awake();
            _parent = FindParentCanvas(transform);
            _childUIObjs = GetComponentsInChildren<UIScaleButton>(true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            BackOverShowUI(true);
        }
        
        private void OverShowUI()
        {
            if (_isOverlay) return;
            OnOverlay?.Invoke(true);
            _canvasGroup = gameObject.EnsureComponent<CanvasGroup>();
            _graphicRaycaster = gameObject.EnsureComponent<GraphicRaycaster>();
            _canvas = gameObject.EnsureComponent<Canvas>();
            _canvas.overrideSorting = true;
            if (null == _parent) _parent = FindParentCanvas(rectTransform);
            _canvas.sortingOrder = _parent.sortingOrder + 1;
            _isOverlay = true;
        }

        public void BackOverShowUI(bool init = false)
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (null != group) DestroyImmediate(group);
            
            GraphicRaycaster raycast = GetComponent<GraphicRaycaster>();
            if (null != raycast) DestroyImmediate(raycast);
            
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null) DestroyImmediate(canvas);
            
            OnOverlay?.Invoke(false);
            _isOverlay = false;
        }

        public override void OnPressOther(GameObject go)
        {
            base.OnPressOther(go);
            if (null != go)
            {
                foreach (var scaleButton in _childUIObjs)
                {
                    if (null != scaleButton && scaleButton.go == go)
                        return;
                }
            }
            BackOverShowUI();
        }

        protected override void OnClick()
        {
            base.OnClick();
            OverShowUI();
        }
    }
}
