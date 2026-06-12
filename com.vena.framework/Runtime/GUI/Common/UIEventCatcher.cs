// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    public delegate void ToggleAction(Toggle toggle, bool isOn);

    /// <summary>
    /// UI事件的统一分发
    /// </summary>
    public class UIEventCatcher : UIElement
    {
        [Tooltip("UI是否可交互")]
        public bool interactable = true;

        public ToggleAction onToggleChange = null;
        public VoidDelegate onClick = null;
        public VoidDelegate onPointEnter = null;
        public VoidDelegate onPointExit = null;
        public VoidDelegate onSelect = null;
        public VoidDelegate onUpdateSelect = null;
        public EventDelegate onPointDown = null;
        public EventDelegate onPointUp = null;
        public EventDelegate onBeginDrag = null;
        public EventDelegate onDraging = null;
        public EventDelegate onDrop = null;
        public EventDelegate onEndDrag = null;

        private void OnToggleChange(Toggle toggle, bool isOn)
        {
            if (interactable) onToggleChange?.Invoke(toggle, isOn);
        }
        private void OnClick(GameObject go)
        {
            if (interactable) onClick?.Invoke(go);
        }
        private void OnPointEnter(GameObject go)
        {
            if (interactable) onPointEnter?.Invoke(go);
        }
        private void OnPointExit(GameObject go)
        {
            if (interactable) onPointExit?.Invoke(go);
        }
        private void OnSelect(GameObject go)
        {
            if (interactable) onSelect?.Invoke(go);
        }
        private void OnUpdateSelect(GameObject go)
        {
            if (interactable) onUpdateSelect?.Invoke(go);
        }
        private void OnPointDown(GameObject go, PointerEventData eventData)
        {
            if (interactable) onPointDown?.Invoke(go, eventData);
        }
        private void OnPointUp(GameObject go, PointerEventData eventData)
        {
            if (interactable) onPointUp?.Invoke(go, eventData);
        }
        private void OnBeginDrag(GameObject go, PointerEventData eventData)
        {
            if (interactable) onBeginDrag?.Invoke(go, eventData);
        }
        private void OnDragging(GameObject go, PointerEventData eventData)
        {
            if (interactable) onDraging?.Invoke(go, eventData);
        }
        private void OnDrop(GameObject go, PointerEventData eventData)
        {
            if (interactable) onDrop?.Invoke(go, eventData);
        }
        private void OnEndDrag(GameObject go, PointerEventData eventData)
        {
            if (interactable) onEndDrag?.Invoke(go, eventData);
        }

        public void InitEvent()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            int length = buttons.Length;
            for (int i = 0; i < length; i++)
            {
                Button button = buttons[i];
                button.onClick.AddListener(() => OnClick(button.gameObject));
            }

            UIScaleButton[] scaleButtons = GetComponentsInChildren<UIScaleButton>(true);
            length = scaleButtons.Length;
            for (int i = 0; i < length; i++)
            {
                UIScaleButton button = scaleButtons[i];
                button.onClick -= OnClick;
                button.onClick += OnClick;
            }

            Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
            length = toggles.Length;
            for (int i = 0; i < length; i++)
            {
                Toggle toggle = toggles[i];
                toggle.onValueChanged.AddListener((b) => OnToggleChange(toggle, b));
            }

            UIEventTrigger[] triggers = GetComponentsInChildren<UIEventTrigger>(true);
            length = triggers.Length;
            for (int i = 0; i < length; i++)
            {
                var trigger = triggers[i];

                trigger.onClick -= OnClick;
                trigger.onClick += OnClick;

                trigger.onEnter -= OnPointEnter;
                trigger.onEnter += OnPointEnter;

                trigger.onExit -= OnPointExit;
                trigger.onExit += OnPointExit;

                trigger.onSelect -= OnSelect;
                trigger.onSelect += OnSelect;

                trigger.onUpdateSelect -= OnUpdateSelect;
                trigger.onUpdateSelect += OnUpdateSelect;

                trigger.onPointerDown -= OnPointDown;
                trigger.onPointerDown += OnPointDown;

                trigger.onPointerUp -= OnPointUp;
                trigger.onPointerUp += OnPointUp;

                trigger.onBeginDrag -= OnBeginDrag;
                trigger.onBeginDrag += OnBeginDrag;

                trigger.onDraging -= OnDragging;
                trigger.onDraging += OnDragging;

                trigger.onEndDrag -= OnEndDrag;
                trigger.onEndDrag += OnEndDrag;

                trigger.onDrop -= OnDrop;
                trigger.onDrop += OnDrop;
            }
        }

        public void ReInitEvent()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            int length = buttons.Length;
            for (int i = 0; i < length; i++)
            {
                Button button = buttons[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnClick(button.gameObject));
            }

            UIScaleButton[] scaleButtons = GetComponentsInChildren<UIScaleButton>(true);
            length = scaleButtons.Length;
            for (int i = 0; i < length; i++)
            {
                UIScaleButton button = scaleButtons[i];
                button.onClick += OnClick;
            }

            Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
            length = toggles.Length;
            for (int i = 0; i < length; i++)
            {
                Toggle toggle = toggles[i];
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((b) => OnToggleChange(toggle, b));
            }

            UIEventTrigger[] triggers = GetComponentsInChildren<UIEventTrigger>(true);
            length = triggers.Length;
            for (int i = 0; i < length; i++)
            {
                var trigger = triggers[i];
                trigger.onClick = OnClick;
                trigger.onEnter = OnPointEnter;
                trigger.onExit = OnPointExit;
                trigger.onSelect = OnSelect;
                trigger.onUpdateSelect = OnUpdateSelect;
                trigger.onPointerDown = OnPointDown;
                trigger.onPointerUp = OnPointUp;
                trigger.onBeginDrag = OnBeginDrag;
                trigger.onDraging = OnDragging;
                trigger.onEndDrag = OnEndDrag;
                trigger.onDrop = OnDrop;
            }
        }
    }

}
