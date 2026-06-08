using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    /// <summary>
    /// 游戏基础架构---UIPanel的子页签类 
    /// 用于UIPanel中设计有多个状态的情况 
    /// Toggle 参数是用于页签组（有且只有一个会显示出来）
    /// </summary>
    public abstract class GuiTabs : GuiBase
    {
        private Toggle _toggle;
        
        private GuiPanel _parentPanel;

        private bool _visible = false;

        public bool visible => _visible;

        public Toggle toggle => _toggle;
        
        public GuiPanel parentPanel => _parentPanel;
        
        internal void Init(GuiPanel guiPanel, Toggle toggle)
        {
            _parentPanel = guiPanel;
            _toggle = toggle;
            _toggle.onValueChanged.RemoveAllListeners();
            _toggle.onValueChanged.AddListener(gameObject.SetActive);
            _toggle.onValueChanged.AddListener(OnSelect);
            //InitEvent(this);
            OnSelect(_toggle == null || _toggle.isOn);
        }
            
        private void OnSelect(bool visible)
        {
            if (_visible == visible) return;
            _visible = visible;
            if (_visible) OnEnter();
            else OnExit();
        }
        
        protected abstract void OnEnter();

        protected abstract void OnExit();
        
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
    }
}
