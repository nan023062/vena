// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

namespace Vena.Framework
{
    /// <summary>
    /// UI界面基类,
    /// 维护UI界面的生命周期,
    /// 实现MVC模式的抽象View,
    /// UI界面中字页签的管理。
    /// </summary>
    public abstract class GuiPanel : GuiBase
    {
        private PanelAttribute _config;
        
        public PanelStyle style => _config.style;

        public bool raycastBlock => _config.raycastBlock;
        
        public RectTransform node { private set; get; }
        
        public void Close()
        {
            GameWorld.ClosePanel(GetType());
        }
        
        #region Depth Controller

        private UIPanel _panel;

        private UIDepth[] _depths;
        
        public UIPanel panel => _panel;
        
        public int depth
        {
            get => panel.depth;
            set
            {
                int min = panel.depth;
                panel.depth = value;
                maxDepth = value;
                for (int i = 0; i < _depths.Length; i++)
                {
                    int depth1 = _depths[i].depth - min + value;
                    _depths[i].depth = depth1;
                    if (depth1 > maxDepth) maxDepth = depth1;
                }
            }
        }
        
        public int maxDepth { private set; get; }

        #endregion
        
        #region Life Cycle

        protected override void OnCreate()
        {
            _config = GetType().GetCustomAttribute<PanelAttribute>();
            _panel = gameObject.GetComponent<UIPanel>();
            _panel.raycastBlock = raycastBlock;
            node = _panel.transNode as RectTransform;
            _depths = GetComponentsInChild<UIDepth>(true);
            InitEvent(_panel);
        }
        
        public void Enable(object args)
        {
            OnOpened(args);
            _panel.interactable = true;
        }
        
        public void Disable()
        {
            _panel.interactable = false;
            OnClosed();
        }
        
        protected abstract void OnOpened(object arg);

        protected abstract void OnClosed();
        
        protected override void OnDestroy() { }
        
        protected override void OnPointEnter(GameObject go) { }

        protected override void OnPointExit(GameObject go) { }

        protected override void OnSelect(GameObject go) { }

        protected override void OnUpdateSelect(GameObject go) { }

        protected override void OnPointDown(GameObject go, PointerEventData eventData) { }

        protected override void OnPointUp(GameObject go, PointerEventData eventData) { }

        protected override void OnBeginDrag(GameObject go, PointerEventData eventData) { }

        protected override void OnDragging(GameObject go, PointerEventData eventData) { }

        protected override void OnDrop(GameObject go, PointerEventData eventData) { }

        protected override void OnEndDrag(GameObject go, PointerEventData eventData) { }

        #endregion

        #region UIObject Tab 

        private readonly List<GuiTabs> _subTabLst = new List<GuiTabs>();
        
        protected T CreateTab<T>(Toggle toggleGroup, GameObject go) where T : GuiTabs, new()
        {
            T tab = World.Default.CreateActor<T>();
            ((IGui)tab).Init(go);
            _subTabLst.Add(tab);
            tab.Init(this, toggleGroup);
            return tab;
        }
        
        #endregion
    }
}
