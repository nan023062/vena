using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    public delegate void OnClickedDropItem(CustomDropItem item);
    
    [RequireComponent(typeof(RectTransform))]
    public class CustomDropItem : UIElement
    {
        public GameObject check;
        public Button Button;

        public event OnClickedDropItem onClick;

        protected override void Awake()
        {
            base.Awake();
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (onClick != null) onClick(this);
        }
    }
}
