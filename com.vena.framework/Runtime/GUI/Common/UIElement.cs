// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using Vena.UnityExtensions;

namespace Vena.Framework
{
    public abstract class UIElement : NodeReferences
    {
        private RectTransform _transform;
        
        private GameObject _gameObject;
        
        public GameObject go => _gameObject;
        
        public RectTransform rectTransform => _transform;
        
        private CanvasGroup _canvasGroup;
        
        private Canvas _canvas;

        protected virtual void Awake()
        {
            _gameObject = gameObject;
            
            _transform = transform as RectTransform;
            
            _uiRoot ??= UIHelper.FindFirstUIRoot(_transform);
        }
        
        protected Transform FindChild(string path)
        {
            return _transform.Find(path);
        }

        protected GameObject FindChildGameObject(string path)
        {
            Transform child = FindChild(path);
            return child == null ? null : child.gameObject;
        }

        protected T GetComponent<T>(string path = null) where T : UnityEngine.Component
        {
            if (string.IsNullOrEmpty(path))
            {
                return gameObject.GetComponent<T>();
            }
            
            GameObject go = FindChildGameObject(path);
            if (go == null) return null;
            return go.GetComponent<T>();
        }

        protected Canvas FindParentCanvas(Transform node)
        {
            Canvas canvas = null;
            if (node.parent == null) return canvas;
            canvas = node.parent.GetComponent<Canvas>();
            canvas ??= FindParentCanvas(node.parent);
            return canvas;
        }

        public void SetOverlay(bool active)
        {
            if (active)
            {
                _canvasGroup = go.GetComponent<CanvasGroup>();
                _canvasGroup ??= go.AddComponent<CanvasGroup>();
                
                _canvas = go.GetComponent<Canvas>();
                _canvas ??= go.AddComponent<Canvas>();
                
                _canvas.overrideSorting = true;
                var canvas = FindParentCanvas(_transform);
                _canvas.sortingOrder = canvas.sortingOrder + 1;
            }
            else
            {
                enabled = false;
                
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup != null)
                {
                    DestroyImmediate(_canvasGroup);
                    _canvasGroup = null;
                }
                _canvas = GetComponent<Canvas>();
                if (_canvas != null)
                {
                    DestroyImmediate(_canvas);
                    _canvas = null;
                }
                
                enabled = true;
            }
        }

        #region Parent UIRoot

        private static UIRoot _uiRoot;

        public bool CheckInRectangle(Vector2 point)
        {
            if (_uiRoot == null) _uiRoot = UIHelper.FindFirstUIRoot(rectTransform);
            return _uiRoot && _uiRoot.CheckInRectangle(rectTransform, point);
        }

        public bool CheckInRectangle(Vector2 point, ref Vector2 local)
        {
            if (_uiRoot == null) _uiRoot = UIHelper.FindFirstUIRoot(rectTransform);
            return _uiRoot && _uiRoot.CheckInRectangle(rectTransform, point, out local);
        }

        public static bool CheckInRectangle(RectTransform rect, Vector2 point)
        {
            if (_uiRoot == null) _uiRoot = UIHelper.FindFirstUIRoot(rect);
            return _uiRoot && _uiRoot.CheckInRectangle(rect, point);
        }

        public static bool CheckInRectangle(RectTransform rect, Vector2 point, ref Vector2 local)
        {
            if (_uiRoot == null) _uiRoot = UIHelper.FindFirstUIRoot(rect);
            return _uiRoot && _uiRoot.CheckInRectangle(rect, point, out local);
        }

        #endregion
    }
}
