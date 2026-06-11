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
    public sealed class UIRoot : MonoBehaviour
    {
        public Canvas canvas_normal;
        public Canvas canvas_focus;
        public Canvas canvas_3d;

        private CanvasScaler _focusScaler;
        private CanvasScaler _normalScaler;
        private CanvasScaler _3dScaler;
        
        public bool checkRaycast = false;
        
        private void Awake()
        {
            canvas_focus.sortingOrder = 0;
            canvas_normal.sortingOrder = 0;
            canvas_3d.sortingOrder = 0;
            
            _focusScaler = canvas_focus.GetComponent<CanvasScaler>();
            _normalScaler = canvas_normal.GetComponent<CanvasScaler>();
            _3dScaler = canvas_3d.GetComponent<CanvasScaler>();
            _focusScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _normalScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _3dScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            
            Adaptation();
        }
        
        public int baseWidth = 1334;
        public int baseHeight = 750;

        private float _renderWidth;
        private float _renderHeight;

        /// <summary>
        /// UI屏幕适配
        /// </summary>
        private void Adaptation()
        {
            if (_focusScaler != null && _normalScaler)
            {
                _focusScaler.referenceResolution = new Vector2(baseWidth, baseHeight);
                _normalScaler.referenceResolution = new Vector2(baseWidth, baseHeight);
                _3dScaler.referenceResolution = new Vector2(baseWidth, baseHeight);

                _renderWidth = Screen.width;
                _renderHeight = Screen.height;
                float baseRatio = baseWidth * 1.0f / baseHeight;

                //大于16：9 例如：IphoneX
                if ((_renderWidth / _renderHeight) > baseRatio)
                {
                    _renderHeight = baseHeight * canvas_normal.transform.lossyScale.x;
                    _renderWidth = _renderHeight * baseRatio;
                    _focusScaler.matchWidthOrHeight = 1;
                    _normalScaler.matchWidthOrHeight = 1;
                    _3dScaler.matchWidthOrHeight = 1;
                }
                //小于16：9 例如：Ipad
                else if ((_renderWidth / _renderHeight) < baseRatio)
                {
                    _renderWidth = baseWidth * canvas_normal.transform.lossyScale.x;
                    _renderHeight = _renderWidth / baseRatio;
                    _focusScaler.matchWidthOrHeight = 0;
                    _normalScaler.matchWidthOrHeight = 0;
                    _3dScaler.matchWidthOrHeight = 0;
                }
            }
        }

#if UNITY_EDITOR

        private void Update()
        {
            Adaptation();
        }

        readonly Vector3[] _corners = new Vector3[4];
        
        private void OnDrawGizmos()
        {
            var pos = canvas_normal.transform.position;
            _corners[0] = new Vector3(-_renderWidth, -_renderHeight, 0) * 0.5f + pos;
            _corners[1] = new Vector3(-_renderWidth, _renderHeight, 0) * 0.5f + pos;
            _corners[2] = new Vector3(_renderWidth, _renderHeight, 0) * 0.5f + pos;
            _corners[3] = new Vector3(_renderWidth, -_renderHeight, 0) * 0.5f + pos;
            DrawRectangle(Color.green);
            
            if (checkRaycast)
            {
                foreach (var maskableGraphic in FindObjectsOfType<MaskableGraphic>())
                {
                    if (maskableGraphic.raycastTarget && maskableGraphic.transform is RectTransform rect1)
                    {
                        rect1.GetWorldCorners(_corners);
                        DrawRectangle(Color.red);
                    }
                }
            }
        }
        
        private void DrawRectangle(Color color)
        {
            Color color1 = Gizmos.color;
            Gizmos.color = color;
            for (int j = 0; j < 4; j++)
            {
                Gizmos.DrawLine(_corners[j], _corners[(j + 1) % 4]);
            }
            Gizmos.color = color1;
        }
        
#endif

        public bool CheckInRectangle(RectTransform rectTransfrom, Vector2 point)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransfrom, point, canvas_normal.worldCamera);
        }

        public bool CheckInRectangle(RectTransform rectTransfrom, Vector2 point, out Vector2 localPos)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransfrom, point,
                canvas_normal.worldCamera, out localPos);
        }
        
        public void ScreenPointToViewport()
        {
        }


    }
}