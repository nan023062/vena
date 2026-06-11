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
    public class UIFollow3DTarget : MonoBehaviour
    {
        public Transform mFollowTarget;
        private Vector3 mFollowOffset = Vector3.zero;
        private UIRoot mUIRoot;
        public RectTransform mParentRectTrans;
        private RectTransform mRectTransCache;

        public static UIFollow3DTarget Get(GameObject go)
        {
            UIFollow3DTarget followMo = go.GetComponent<UIFollow3DTarget>();
            if (followMo == null) followMo = go.AddComponent<UIFollow3DTarget>();
            followMo.mUIRoot = UIHelper.FindUIRoot();
            followMo.mParentRectTrans = followMo.mUIRoot.canvas_3d.GetComponent<RectTransform>();
            followMo.mRectTransCache = go.GetComponent<RectTransform>();
            followMo.mRectTransCache.localScale = Vector3.one;
            followMo.mParentRectTrans.localRotation = Quaternion.identity;
            followMo.mParentRectTrans.anchoredPosition = Vector2.zero;
            return followMo;
        }

        public void SetFollowObject(Transform followObject, Vector3 offset)
        {
            mFollowOffset = offset;
            mFollowTarget = followObject;
            Update();
        }

        private void Update()
        {
            if (mFollowTarget != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(mFollowTarget.position + mFollowOffset);
                Vector2 localPos = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mParentRectTrans, screenPos, mUIRoot.canvas_normal.worldCamera, out localPos);
                mRectTransCache.anchoredPosition = localPos;
            }
        }
    }
}

