using System;
using UnityEngine;

namespace Vena.Framework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIRectAdaptation : MonoBehaviour
    {
        public enum Corner
        {
            LeftBotton = 0,
            LeftTop = 1,
            RightTop = 2,
            RightBotton = 3,
        }

        private CanvasGroup mCanvasGroup;
        private RectTransform mRectTransform;
        private Vector3[] mAdapteCorners = new Vector3[4];
        private Vector3[] mTargetCorners = new Vector3[4];
        private Vector3[] mInnerCorners = new Vector3[4];
        private Corner mCorner = Corner.RightBotton;

        /// <summary>
        /// 需要适配的Rect范围
        /// </summary>
        [SerializeField]
        private RectTransform mInnerRect;

        /// <summary>
        /// 边距 (对适配范围的补充)
        /// </summary>
        [SerializeField]
        private float mPadding = 0;

        /// <summary>
        /// 需要跟随的UI
        /// </summary>
        [SerializeField]
        private RectTransform mTargetRect;

        /// <summary>
        /// 需要跟随的UI
        /// </summary>
        [SerializeField]
        private RectTransform mAdaptingRect;

        [ExecuteInEditMode]
        private void Awake()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (mCanvasGroup == null)
            {
                mCanvasGroup = GetComponent<CanvasGroup>();
            }
            if (mInnerRect == null)
            {
                mInnerRect = transform.parent as RectTransform;
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
            this.mInnerRect = innerRect;
            this.mPadding = padding;
        }

        /// <summary>
        /// 设置需要适配的UI参数
        /// </summary>
        public void SetAdapteUI(RectTransform targetRect, RectTransform adaptingRect, int corner)
        {
            Initialized();
            mTargetRect = targetRect;
            mAdaptingRect = adaptingRect;
            mCorner = (Corner)(System.Math.Max(System.Math.Min(3,corner),0));
            mCanvasGroup.alpha = 0;

            if (mAdaptingRect != null && targetRect != null)
            {
                OnUpdateCorner();
                mAdaptingRect.position = mTargetCorners[GetCorner()];

                Adaptating();
                mCanvasGroup.alpha = 1;
            }
        }

        public int GetAdapteCorner()
        {
            int index = GetCorner();
            if (index >= 2) return index - 2;
            else return index + 2;
           
        }

        public int GetCorner()
        {
            return (int)mCorner;
        }

        private void Adaptating()
        {
            Initialized();

            if (mAdaptingRect == null || mTargetRect == null) return;

            Vector3 worldScale = mAdaptingRect.lossyScale;
            if (Mathf.Approximately(worldScale.x * worldScale.y, 0)) return;

            //获取目标的四角坐标
            mTargetRect.GetWorldCorners(mTargetCorners);
            
            //适配到Rect范围内
            if (mInnerRect != null)
            {
                mInnerRect.GetWorldCorners(mInnerCorners);
                mAdaptingRect.GetWorldCorners(mAdapteCorners);

                if (!CheckInner(mCorner))
                {
                    if (mCorner != Corner.RightBotton && CheckInner(Corner.RightBotton))
                    {
                        mCorner = Corner.RightBotton;
                        OnUpdateCorner();
                    }
                    else if (mCorner != Corner.LeftBotton && CheckInner(Corner.LeftBotton))
                    {
                        mCorner = Corner.LeftBotton;
                        OnUpdateCorner();
                    }
                    else if (mCorner != Corner.RightTop && CheckInner(Corner.RightTop))
                    {
                        mCorner = Corner.RightTop;
                        OnUpdateCorner();
                    }
                    else if (mCorner != Corner.LeftTop && CheckInner(Corner.LeftTop))
                    {
                        mCorner = Corner.LeftTop;
                        OnUpdateCorner();
                    }
                }
            }

            Vector3 newPos = mTargetCorners[GetCorner()];
            Vector3 oldPos = mAdaptingRect.position;
            Vector3 delta = oldPos - newPos;
            if (Vector3.Dot(delta, delta) > 0.001)
                mAdaptingRect.position = newPos;
        }

        private bool CheckInner(Corner corner)
        {
            Vector3 position = mTargetCorners[(int)corner];
            Vector2 size = mAdapteCorners[2] - mAdapteCorners[0];
            Vector3 worldPadding = mInnerRect.lossyScale * mPadding;
            Vector2 inMin = mInnerCorners[0] + worldPadding;
            Vector2 inMax = mInnerCorners[2] - worldPadding;

            Vector2 min = position;
            Vector2 max = position;

            switch (corner)
            {
                case Corner.RightTop:
                    max += size;
                    break;
                case Corner.RightBotton:
                    max.x += size.x;
                    min.y -= size.y;
                    break;
                case Corner.LeftBotton:
                    min -= size;
                    break; 
                case Corner.LeftTop:
                    max.y += size.y;
                    min.x -= size.x;
                    break;
            }

            return (min.x >= inMin.x && min.y >= inMin.y && max.x <= inMax.x && max.y <= inMax.y);
        }

        private void OnUpdateCorner()
        {
            if (mAdaptingRect != null)
            {
                switch ((Corner)GetAdapteCorner())
                {
                    case Corner.LeftBotton:
                        mAdaptingRect.pivot = new Vector2(0, 0);
                        break;
                    case Corner.LeftTop:
                        mAdaptingRect.pivot = new Vector2(0, 1);
                        break;
                    case Corner.RightTop:
                        mAdaptingRect.pivot = new Vector2(1, 1);
                        break;
                    case Corner.RightBotton:
                        mAdaptingRect.pivot = new Vector2(1, 0);
                        break;
                    default:
                        break;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetInnerRect(mInnerRect, mPadding);
            SetAdapteUI(mTargetRect, mAdaptingRect, (int)mCorner);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3[] __coners = new Vector3[4];
            mInnerRect.GetWorldCorners(__coners);
            Vector3 padding = mInnerRect.lossyScale * mPadding;
 
            __coners[0] += new Vector3(padding.x, padding.y);
            __coners[1] += new Vector3(padding.x, -padding.y);
            __coners[2] += new Vector3(-padding.x, -padding.y);
            __coners[3] += new Vector3(-padding.x, padding.y);

            for (int i = 0; i < 5; i++)
            {
                Gizmos.DrawLine(__coners[i % 4], __coners[(i + 1) % 4]);
            }

        }
#endif
    }
}

