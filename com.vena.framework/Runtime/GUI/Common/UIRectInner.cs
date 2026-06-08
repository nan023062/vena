
using UnityEngine;

namespace Vena.Framework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIRectInner : MonoBehaviour
    {
        /// <summary>
        /// 本身RectTransform
        /// </summary>
        private RectTransform mRectTrans;

        /// <summary>
        /// 需要适配的Rect范围
        /// </summary>
        [SerializeField]
        private RectTransform innerRect;

        private CanvasGroup mCanvasGroup;

        /// <summary>
        /// 边距 (对适配范围的补充)
        /// </summary>
        [SerializeField]
        private float padding = 10;

        private Vector3[] fourCorners = new Vector3[4];

        /// <summary>
        /// 需要跟随的UI
        /// </summary>
        [SerializeField]
        private RectTransform mTargetUI;

        private void Initialized()
        {
            if (mRectTrans == null)
            {
                mRectTrans = GetComponent<RectTransform>();
                mRectTrans.anchorMin = Vector2.one * 0.5f;
                mRectTrans.anchorMax = Vector2.one * 0.5f;
                mRectTrans.pivot = Vector2.one;
            }

            if (mCanvasGroup == null)
            {
                mCanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Update()
        {
            Adaptating();
        }

        /// <summary>
        /// 设置适配的范围参数
        /// </summary>
        public void SetInnerRect(RectTransform innerRect, float padding)
        {
            Initialized();
            this.innerRect = innerRect;
            this.padding = padding;
            Adaptating();
        }

        /// <summary>
        /// 设置跟随UI
        /// </summary>
        public void SetTarget(RectTransform target)
        {
            Initialized();
            mCanvasGroup.alpha = 0;
            mTargetUI = target;
            if (mTargetUI != null)
            {
                Adaptating();
                mCanvasGroup.alpha = 1;
                Adaptating();
            }
        }

        private void Adaptating()
        {
            Initialized();
            AdjustmentSize();
            AdaptationInner();
        }

        private void AdjustmentSize()
        {
            Vector2 lossyScale = mRectTrans.lossyScale;
            if (lossyScale.x <= 0 || lossyScale.y <= 0) return;

            Vector2 min = Vector2.one * float.MaxValue;
            Vector2 max = Vector2.one * float.MinValue;
            int length = mRectTrans.childCount;
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    var childRect = mRectTrans.GetChild(i) as RectTransform;
                    var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(mRectTrans, childRect);
                    min.x = Mathf.Min(min.x, bounds.min.x);
                    min.y = Mathf.Min(min.y, bounds.min.y);
                    max.x = Mathf.Max(max.x, bounds.max.x);
                    max.y = Mathf.Max(max.y, bounds.max.y);
                }

                Vector2 pivot = mRectTrans.pivot;
                Vector2 sizeDelta = Vector2.one * 100;
                Vector2 offset = Vector2.zero;
                if (max.x > min.x && max.y > min.y)
                {
                    sizeDelta = (max - min);
                    offset.x = Mathf.Lerp(min.x, max.x, pivot.x);
                    offset.y = Mathf.Lerp(min.y, max.y, pivot.y);
                }
                mRectTrans.sizeDelta = sizeDelta;

                if (offset.x != 0 || offset.y != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        var childRect = mRectTrans.GetChild(i) as RectTransform;
                        childRect.anchoredPosition -= offset;
                    }
                }
                mRectTrans.anchoredPosition += offset;
            }
        }

        public int GetCornerIndex()
        {
            Vector2 pivot = mRectTrans.pivot;
            int corner = 0;
            if (pivot.x < 0.01 && pivot.y < 0.01) corner = 0;
            else if (pivot.x < 0.01 && pivot.y > 0.99) corner = 1;
            else if (pivot.x > 0.99 && pivot.y > 0.99) corner = 2;
            else if (pivot.x > 0.99 && pivot.y < 0.01) corner = 3;
            return corner;
        }

        private void AdaptationInner()
        {
            if (mTargetUI != null)
            {
                Vector2 pivot = mRectTrans.pivot;
                pivot.x = Mathf.RoundToInt(pivot.x);
                pivot.y = Mathf.RoundToInt(pivot.y);
                UpdatePosition(pivot);
            }

            if (innerRect != null)
            {
                Vector2 sizeDelta = innerRect.rect.size;
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(innerRect, mRectTrans);

                Vector2 pivot = mRectTrans.pivot;
                Vector2 halfsizeDelta = sizeDelta * 0.5f;

                //超出左边界
                if (bounds.min.x < padding - halfsizeDelta.x)
                {
                    pivot.x = 0;
                    if (CheckInner(pivot)) return;
                }
                //超出右边界
                else if (bounds.max.x > halfsizeDelta.x - padding)
                {
                    pivot.x = 1;
                    if (CheckInner(pivot)) return;
                }

                //超出下边界
                if (bounds.min.y < padding - halfsizeDelta.y)
                {
                    pivot.y = 0;
                    if (CheckInner(pivot)) return;
                }
                //超出上边界
                else if (bounds.max.y > halfsizeDelta.y - padding)
                {
                    pivot.y = 1;
                    if (CheckInner(pivot)) return;
                }
            }
        }

        private bool CheckInner(Vector2 pivot)
        {
            if (innerRect != null)
            {
                Vector3 lastPostion = mRectTrans.position;
                if(mTargetUI != null) UpdatePosition(pivot);

                Vector2 lastPivot = mRectTrans.pivot;
                mRectTrans.pivot = pivot;
                Vector2 sizeDelta = innerRect.rect.size;
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(innerRect, mRectTrans);
                if (bounds.size.x + padding > sizeDelta.x || bounds.size.y + padding > sizeDelta.y)
                {
                    UnityEngine.Debug.LogErrorFormat("UIRectInner的内容超出了限定的Rect大小！");
                    mRectTrans.pivot = lastPivot;
                    mRectTrans.position = lastPostion;
                    return false;
                }

                Vector2 halfsizeDelta = sizeDelta * 0.5f;
      
                if (bounds.min.x < padding - halfsizeDelta.x ||     //超出左边界
                    bounds.max.x > halfsizeDelta.x - padding ||     //超出右边界
                    bounds.min.y < padding - halfsizeDelta.y ||     //超出下边界
                    bounds.max.y > halfsizeDelta.y - padding)       //超出上边界
                {
                    mRectTrans.pivot = lastPivot;
                    mRectTrans.position = lastPostion;
                    return false;
                }
            }
            return true;
        }

        private void UpdatePosition(Vector2 pivot)
        {
            if (mTargetUI != null)
            {
                int corner = 0;
                if (pivot.x < 0.01 && pivot.y < 0.01) corner = 2;
                else if (pivot.x < 0.01 && pivot.y > 0.99) corner = 3;
                else if (pivot.x > 0.99 && pivot.y > 0.99) corner = 0;
                else if (pivot.x > 0.99 && pivot.y < 0.01) corner = 1;

                mTargetUI.GetWorldCorners(fourCorners);
                Vector3 postion = fourCorners[corner];

                Vector3 delta = mRectTrans.position - postion;
                if (Vector3.Dot(delta, delta) > 0.1f)
                {
                    mRectTrans.position = postion;
                }         
            }
        }
    }
}
