// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Framework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class UICustomEvent : UIElement
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void OnEnable()
        {
            UICustomEventSystem.AddUIEvent(this);
        }

        protected virtual void OnDisable()
        {
            UICustomEventSystem.RemoveUIEvent(this);
        }

        public abstract void OnPressed(Vector2 positio);

        public abstract void OnPressOther(GameObject go);

        public abstract void OnRelease(Vector2 positio);

        public abstract void OnBeginDrag(Vector2 positio, Vector2 delta);

        public abstract void OnDragging(Vector2 position, Vector2 delta);

        public abstract void OnEndDrag(Vector2 positio);

    }
}
