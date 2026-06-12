// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Framework
{ 
    public static class UIHelper
    {
        static UIRoot _uiRoot;
        
        public static UIRoot FindFirstUIRoot(Transform transform)
        {
            var parent = transform.parent;
            if (parent != null)
            {
                UIRoot result = parent.GetComponent<UIRoot>();
                if (result == null)
                {
                    result = FindFirstUIRoot(parent);
                }
                return result;
            }
            return null;
        }

        public static UIRoot FindUIRoot()
        {
            if(_uiRoot == null)
            {
                var uiRootObj = GameObject.Find("UIRoot");
                if (uiRootObj != null) _uiRoot = uiRootObj.GetComponent<UIRoot>();
            }
            return _uiRoot;
        }
    }
}
