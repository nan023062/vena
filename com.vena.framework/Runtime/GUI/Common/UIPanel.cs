// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable InconsistentNaming

namespace Vena.Framework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    [RequireComponent(typeof(Image),typeof(GraphicRaycaster))]
    public sealed class UIPanel : UIEventCatcher
    {
        [HideInInspector]
        public Canvas canvas = null;

        [HideInInspector]
        public CanvasGroup canvasGroup = null;
        
        [SerializeField, Range(0, 1)]
        public float renderAlpha = 1f;

        public bool raycastBlock = true;

        public int depth
        {
            get => canvas.sortingOrder;
            set => canvas.sortingOrder = value;
        }

        [Tooltip("UI的背景版，图片组件")]
        public Image board;

        public Transform transNode;

        public RectTransform nodeRect => transNode as RectTransform;
        
        protected override void Awake()
        {
            base.Awake();
            canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.ignoreParentGroups = false;
            renderAlpha = 1;
            board = GetComponent<Image>();
            board.raycastTarget = true;
#if UNITY_EDITOR
            OnValidate();
            
            nodeRect.SpreadFormat();

            if (!name.StartsWith("ui_view_"))
            {
                name = "ui_view_XX(格式)";
            }
#endif      
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!canvas.enabled) canvas.enabled = true;
            if (board.enabled != raycastBlock) board.enabled = raycastBlock;
            if (System.Math.Abs(canvasGroup.alpha - renderAlpha) > 0.0001f) canvasGroup.alpha = renderAlpha;

            if (transNode == null)
            {
                GameObject go = new GameObject("node", typeof(RectTransform));
                transNode = go.transform;
                nodeRect.SpreadFormat(transform);
            }
        }
#endif
        #region 存在子Panel的情况

        private UIDepth[] _depthChildren;
        
        private ArraySegment<UIDepth> GetUIDepthArray(bool force = false)
        {
            if (_depthChildren == null || force)
            {
                _depthChildren = GetComponentsInChildren<UIDepth>(true);
            }

            return new ArraySegment<UIDepth>(_depthChildren);
        }

        /// <summary>
        /// 设置新的起点层级Depth，返回值是当前界面最高Depth
        /// </summary>
        public int SetPanelDepth(int depth, bool force = false)
        {
            int minValue = this.depth;
            this.depth = depth;
            int maxValue = this.depth;
            
            foreach (var uiDepth in GetUIDepthArray(force))
            {
                uiDepth.depth = depth + uiDepth.depth - minValue;
                if (maxValue < uiDepth.depth)
                    maxValue = uiDepth.depth;
            }
            return maxValue;
        }

        #endregion
    }


}
