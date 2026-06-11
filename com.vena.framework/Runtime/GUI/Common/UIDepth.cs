// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    [RequireComponent(typeof(Image), typeof(GraphicRaycaster))]
    public class UIDepth : MonoBehaviour
    {
        [HideInInspector]
        public Canvas canvas;

        [HideInInspector]
        public CanvasGroup canvasGroup;

        [HideInInspector]
        public GraphicRaycaster raycaster;

        [HideInInspector]
        public Image fullBlockImage;

        [SerializeField, Range(0, 1)]
        public float renderAlpha = 1f;

        public bool raycastBlock;
        
        public RectTransform rectTransform { private set; get; }

        public int depth
        {
            get => canvas.sortingOrder;
            set => canvas.sortingOrder = value;
        }
        
        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            renderAlpha = 1;
            rectTransform = transform as RectTransform;
            raycaster = GetComponent<GraphicRaycaster>();
            fullBlockImage = GetComponent<Image>();
            fullBlockImage.raycastTarget = true;
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

#if UNITY_EDITOR
            OnValidate();
#endif      
        }
        
        protected virtual void OnValidate()
        {
            if (!canvas.enabled) canvas.enabled = true;
            if (!raycaster.enabled) raycaster.enabled = true;
            fullBlockImage.enabled = raycastBlock;
            canvasGroup.alpha = renderAlpha;
        }
    }
}
