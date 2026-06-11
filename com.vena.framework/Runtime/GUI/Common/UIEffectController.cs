// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Vena.Framework
{
    [RequireComponent(typeof(Image))]
    public class UIEffectController : UIElement
    {
        private Image _targetGraphic;
        [SerializeField] int _targetQueue;
        private Renderer[] _renderers;
        
        protected override void Awake()
        {
            base.Awake();
            
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
            _targetGraphic = GetComponent<Image>();
            _targetGraphic.color = Color.clear;
            UpdateRendererQueue();
        }
        
        private void LateUpdate()
        {
            UpdateRendererQueue();
        }

        private void UpdateRendererQueue()
        {
            if(_targetQueue != _targetGraphic.material.renderQueue)
            {
                _targetQueue = _targetGraphic.material.renderQueue;

                foreach (var renderer in _renderers)
                {
                    renderer.material.renderQueue = _targetQueue;
                }
            }
        }
    }

}
