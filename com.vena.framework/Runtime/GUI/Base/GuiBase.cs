// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    public interface IGui
    {
        void Init(GameObject go);
    }
    
    public abstract class GuiBase : Actor, IGui, IDestroy
    {
        private GameObject _gameObject;

        public GameObject gameObject => _gameObject;

        
        private RectTransform _transform;

        public RectTransform transform => _transform;

        void IGui.Init(GameObject go)
        {
            _gameObject = go;
            _gameObject.name = $"{GetType().Name}";
            _transform = _gameObject.transform as RectTransform;
            OnCreate();
        }
        
        void IDestroy.OnDestroy()
        {
            OnDestroy();
        }
            
        protected abstract void OnCreate();

        protected abstract void OnDestroy();

        #region Common Methods

        protected Transform FindChild(string path)
        {
            return _transform.Find(path);
        }

        protected T GetComponent<T>(string path = null) where T : UnityEngine.Component
        {
            if (string.IsNullOrEmpty(path))
            {
                return gameObject.GetComponent<T>();
            }
            Transform go = FindChild(path);
            if (go == null) return null;
            return go.GetComponent<T>();
        }

        protected T[] GetComponentsInChild<T>(bool includeInactive = false) where T : UnityEngine.Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive);
        }

        protected List<GameObject> GetAllChild(string name)
        {
            return gameObject.FindAllChild(name);
        }

        #endregion

        #region Event Function

        protected void InitEvent(UIEventCatcher eventCatcher)
        {
            eventCatcher.onClick = OnClick;
            eventCatcher.onPointEnter = OnPointEnter;
            eventCatcher.onPointExit = OnPointExit;
            eventCatcher.onSelect = OnSelect;
            eventCatcher.onUpdateSelect = OnUpdateSelect;
            eventCatcher.onPointDown = OnPointDown;
            eventCatcher.onPointUp = OnPointUp;
            eventCatcher.onBeginDrag = OnBeginDrag;
            eventCatcher.onEndDrag = OnEndDrag;
            eventCatcher.onDraging = OnDragging;
            eventCatcher.onDrop = OnDrop;
            eventCatcher.InitEvent();
        }

        protected abstract void OnClick(GameObject go);

        protected abstract void OnPointEnter(GameObject go);

        protected abstract void OnPointExit(GameObject go);

        protected abstract void OnSelect(GameObject go);

        protected abstract void OnUpdateSelect(GameObject go);

        protected abstract void OnPointDown(GameObject go, PointerEventData eventData);

        protected abstract void OnPointUp(GameObject go, PointerEventData eventData);

        protected abstract void OnBeginDrag(GameObject go, PointerEventData eventData);

        protected abstract void OnDragging(GameObject go, PointerEventData eventData);

        protected abstract void OnDrop(GameObject go, PointerEventData eventData);

        protected abstract void OnEndDrag(GameObject go, PointerEventData eventData);

        #endregion
    }
}