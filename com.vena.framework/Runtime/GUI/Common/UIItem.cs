/**********************************************************************************
 * FileName:    UIItem.cs
 * Description: UIItem的绑定UI组件
 *          1 记录UI的节点（减少代码逻辑） 
 *          2 处理背景Mask
 *          3 统一处理UI事件 
 *********************************************************************************/
using UnityEngine;

namespace Vena.Framework
{
    [ExecuteInEditMode]
    public sealed class UIItem : UIEventCatcher
    {
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (!name.StartsWith("ui_item_"))
                name = "ui_item_XX(格式)";
#endif
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}
