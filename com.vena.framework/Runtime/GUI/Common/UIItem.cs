// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
