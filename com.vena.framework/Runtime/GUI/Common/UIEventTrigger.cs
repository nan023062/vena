// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    public delegate void EventDelegate(GameObject go, PointerEventData eventData);
    public delegate void VoidDelegate(GameObject go);

    public class UIEventTrigger : EventTrigger
    {
        public VoidDelegate onClick;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelect;

        public EventDelegate onPointerDown;
        public EventDelegate onPointerUp;
        public EventDelegate onBeginDrag;
        public EventDelegate onDraging;
        public EventDelegate onEndDrag;
        public EventDelegate onDrop;

        static public UIEventTrigger Get(UIBehaviour ui)
        {
            GameObject go = ui.gameObject;
            UIEventTrigger listener = go.GetComponent<UIEventTrigger>();
            if (listener == null) listener = go.AddComponent<UIEventTrigger>();
            return listener;
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null) onClick(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onPointerDown != null) onPointerDown(gameObject,eventData);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null) onEnter(gameObject);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null) onExit(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onPointerUp != null) onPointerUp(gameObject,eventData);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(gameObject);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(gameObject);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (onDraging != null) onDraging(gameObject,eventData);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag(gameObject, eventData);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(gameObject, eventData);
        }
        public override void OnDrop(PointerEventData eventData)
        {
            if (onDrop != null) onDrop(gameObject,eventData);
        }
    }
}