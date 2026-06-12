// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vena.Framework
{
    public enum ScrollDirection
    {
        Horizontal,
        Vertical
    }

    public delegate void OnScrollItemShow(ScrollItem item, int index);

    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class LoopScrollView : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        public enum MovementType
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport,
        }

        #region LoopScrollView Member

        public ScrollItemPool itemPool;

        public OnScrollItemShow itemShowHandler;

        public int totalCount;
        [HideInInspector] public bool initInStart = false;

        public float threshold = 100; //阀值，当移动超出次范围时会回收item

        public Func<Vector2, bool> canDrag;

        private int m_StartIndex = 0;
        protected int m_EndIndex = 0;

        private float m_ContentSpacing = -1;

        public float contentSpacing
        {
            get
            {
                if (m_ContentSpacing >= 0)
                {
                    return m_ContentSpacing;
                }

                m_ContentSpacing = 0;
                if (content != null)
                {
                    HorizontalOrVerticalLayoutGroup layout1 = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if (layout1 != null)
                    {
                        m_ContentSpacing = layout1.spacing;
                    }
                    else
                    {
                        GridLayoutGroup layout2 = content.GetComponent<GridLayoutGroup>();
                        if (layout2 != null)
                        {
                            m_ContentSpacing = GetDimension(layout2.spacing);
                        }
                    }
                }

                return m_ContentSpacing;
            }
        }

        private int m_ContentConstraintCount = 0;

        protected int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCount > 0)
                {
                    return m_ContentConstraintCount;
                }

                m_ContentConstraintCount = 1;
                if (content != null)
                {
                    GridLayoutGroup layout2 = content.GetComponent<GridLayoutGroup>();
                    if (layout2 != null)
                    {
                        if (layout2.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Debug.Warning("[LoopScrollRect] Flexible not supported yet");
                        }

                        m_ContentConstraintCount = layout2.constraintCount;
                    }
                }

                return m_ContentConstraintCount;
            }
        }

        public bool IsFull
        {
            get
            {
                if (m_EndIndex >= totalCount)
                    return true;
                return false;
            }
        }

        private RectTransform m_thisRect;

        protected RectTransform thisRect
        {
            get
            {
                if (m_thisRect == null)
                    m_thisRect = transform.GetComponent<RectTransform>();
                return m_thisRect;
            }
        }

        private Vector3[] mCorners = new Vector3[4];

        public Vector3[] worldCorners
        {
            get
            {
                Vector2 size = thisRect.sizeDelta;
                float x0 = -0.5f * size.x;
                float y0 = -0.5f * size.y;
                float x1 = x0 + size.x;
                float y1 = y0 + size.y;

                Vector3[] localvector3 = new Vector3[4];

                localvector3[0] = new Vector3(x0, y0);
                localvector3[1] = new Vector3(x0, y1);
                localvector3[2] = new Vector3(x1, y1);
                localvector3[3] = new Vector3(x1, y0);

                Vector3 localcenter = thisRect.anchoredPosition3D;
                localvector3[0] += localcenter;
                localvector3[1] += localcenter;
                localvector3[2] += localcenter;
                localvector3[3] += localcenter;

                mCorners[0] = transform.TransformPoint(localvector3[0]);
                mCorners[1] = transform.TransformPoint(localvector3[1]);
                mCorners[2] = transform.TransformPoint(localvector3[2]);
                mCorners[3] = transform.TransformPoint(localvector3[3]);

                return mCorners;
            }
        }

        #endregion

        #region LoopScrollView property set

        private float GetSize(RectTransform rect)
        {
            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    return rect.sizeDelta.x + contentSpacing;
                case ScrollDirection.Vertical:
                    return rect.sizeDelta.y + contentSpacing;
            }

            return 0;
        }

        private float GetDimension(Vector2 vector)
        {
            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    return vector.x;
                case ScrollDirection.Vertical:
                    return vector.y;
            }

            return 0;
        }

        private Vector2 GetVector(float value)
        {
            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    return Vector2.left * value;
                case ScrollDirection.Vertical:
                    return Vector2.up * value;
            }

            return Vector2.zero;
        }

        #endregion

        #region ScrollItem Operation

        private bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            bool needNewAtStart = false;
            bool needNewAtEnd = false;
            bool needDelStart = false;
            bool needDelEnd = false;

            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    needNewAtStart = viewBounds.min.x < contentBounds.min.x;
                    needNewAtEnd = viewBounds.max.x > contentBounds.max.x;
                    needDelStart = viewBounds.min.x > contentBounds.min.x + threshold;
                    needDelEnd = viewBounds.max.x < contentBounds.max.x - threshold;
                    break;
                case ScrollDirection.Vertical:
                    needNewAtStart = viewBounds.max.y > contentBounds.max.y;
                    needNewAtEnd = viewBounds.min.y < contentBounds.min.y;
                    needDelStart = viewBounds.max.y < contentBounds.max.y - threshold;
                    needDelEnd = viewBounds.min.y > contentBounds.min.y + threshold;
                    break;
            }

            if (needNewAtEnd)
            {
                float size = NewItemAtEnd();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }

                    changed = true;
                }
            }
            else if (needDelEnd)
            {
                float size = DeleteItemAtEnd();
                if (size > 0)
                {
                    changed = true;
                }
            }

            if (needNewAtStart)
            {
                float size = NewItemAtStart();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }

                    changed = true;
                }
            }
            else if (needDelStart)
            {
                float size = DeleteItemAtStart();
                if (size > 0)
                {
                    changed = true;
                }
            }

            return changed;
        }

        public int GetItemIndex(RectTransform rect)
        {
            int siblingindex = rect.GetSiblingIndex();
            return m_StartIndex + siblingindex;
        }

        public RectTransform GetItemRect(int index)
        {
            if (index >= m_StartIndex && index < m_EndIndex)
            {
                return content.GetChild(index - m_StartIndex).GetComponent<RectTransform>();
            }

            return null;
        }

        public ScrollItem GetLoopCell(int index)
        {
            if (index >= m_StartIndex && index < m_EndIndex)
            {
                return content.GetChild(index - m_StartIndex).GetComponent<ScrollItem>();
            }

            return null;
        }

        public ScrollItem GetContentChild(int siblingIndex)
        {
            ScrollItem rectTrans = content.GetChild(siblingIndex).GetComponent<ScrollItem>();
            return rectTrans;
        }

        private void RefillScrollItems(int startIdx = 0)
        {
            if (Application.isPlaying)
            {
                initInStart = false;
                StopMovement();
                m_StartIndex = startIdx;
                m_EndIndex = m_StartIndex;

                int len = content.childCount;
                while (content.childCount > 0)
                {
                    itemPool.RecyleItem<ScrollItem>(content.GetChild(0).gameObject);
                }

                BoundsCell();

                initInStart = true;
            }
        }

        private void BoundsCell()
        {
            if (content.childCount > 0)
            {
                Vector2 pos = content.anchoredPosition;
                switch (direction)
                {
                    case ScrollDirection.Horizontal:
                        pos.x = 0;
                        break;
                    case ScrollDirection.Vertical:
                        pos.y = 0;
                        break;
                }

                content.anchoredPosition = pos;
                UpdateBounds();
            }
        }

        private float NewItemAtStart()
        {
            if (totalCount >= 0 && m_StartIndex - contentConstraintCount < 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                m_StartIndex--;
                RectTransform newItem = InstantiateNextItem(m_StartIndex).cacheRectTransform;
                newItem.SetAsFirstSibling();
                size = Mathf.Max(GetSize(newItem), size);
            }

            Vector2 offset = GetVector(size);
            content.anchoredPosition += offset;
            m_PrevPosition += offset;
            m_ContentStartPosition += offset;
            return size;
        }

        private float DeleteItemAtStart()
        {
            if ((totalCount >= 0 && m_EndIndex >= totalCount - 1) || content.childCount == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(0) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                itemPool.RecyleItem<ScrollItem>(oldItem.gameObject);

                m_StartIndex++;

                if (content.childCount == 0)
                {
                    break;
                }
            }

            Vector2 offset = GetVector(size);
            content.anchoredPosition -= offset;
            m_PrevPosition -= offset;
            m_ContentStartPosition -= offset;
            return size;
        }

        private float NewItemAtEnd()
        {
            if (totalCount >= 0 && m_EndIndex >= totalCount)
            {
                return 0;
            }

            float size = 0;
            int si = m_EndIndex % contentConstraintCount;
            for (int i = si; i < contentConstraintCount; i++)
            {
                RectTransform newItem = InstantiateNextItem(m_EndIndex).cacheRectTransform;
                size = Mathf.Max(GetSize(newItem), size);
                m_EndIndex++;
                if (totalCount >= 0 && m_EndIndex >= totalCount)
                {
                    break;
                }
            }

            return size;
        }

        private float DeleteItemAtEnd()
        {
            if ((totalCount >= 0 && m_StartIndex < contentConstraintCount) || content.childCount == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(content.childCount - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);

                itemPool.RecyleItem<ScrollItem>(oldItem.gameObject);

                m_EndIndex--;
                if (m_EndIndex % contentConstraintCount == 0 || content.childCount == 0)
                {
                    break;
                }
            }

            return size;
        }

        private ScrollItem InstantiateNextItem(int itemIdx)
        {
            if (itemPool == null)
            {
                return null;
            }

            ScrollItem nextItem = itemPool.FetchItem<ScrollItem>();
            nextItem.gameObject.SetActive(true);
            nextItem.cacheTransform.SetParent(content, false);
            if (itemShowHandler != null)
            {
                itemShowHandler(nextItem, itemIdx);
            }

            return nextItem;
        }

        #endregion

        #region Provider

        public void Add(int idx)
        {
            if (Application.isPlaying)
            {
                totalCount++;
                if (idx < m_StartIndex)
                    idx = m_StartIndex;

                if (idx >= m_StartIndex && idx < m_EndIndex)
                {
                    int index = idx - m_StartIndex;
                    int si = m_EndIndex % contentConstraintCount; //GridLayout中海油多少元素无法摆满

                    m_EndIndex++;
                    ScrollItem newItem = InstantiateNextItem(idx);
                    newItem.cacheRectTransform.SetSiblingIndex(index);

                    bool needRecyle = false;
                    switch (direction)
                    {
                        case ScrollDirection.Horizontal:
                            needRecyle = m_ViewBounds.min.x < m_ContentBounds.min.x + 1 && si == 0;
                            break;
                        case ScrollDirection.Vertical:
                            needRecyle = m_ViewBounds.min.y > m_ContentBounds.min.y + 1 && si == 0;
                            break;
                    }

                    if (needRecyle)
                    {
                        ScrollItem oldItem = GetContentChild(content.childCount - 1);
                        itemPool.RecyleItem<ScrollItem>(oldItem.cacheObejct);
                        m_EndIndex--;
                    }
                }
            }
        }

        public void Remove(int idx)
        {
            if (idx >= totalCount)
                return;
            if (Application.isPlaying)
            {
                totalCount--;
                if (idx < m_StartIndex)
                    idx = m_StartIndex;
                if (idx >= m_StartIndex && idx < m_EndIndex)
                {
                    int index = idx - m_StartIndex;
                    ScrollItem oldItem = GetContentChild(index);
                    itemPool.RecyleItem<ScrollItem>(oldItem.cacheObejct);
                    m_EndIndex--;

                    bool needNewItem = false;
                    switch (direction)
                    {
                        case ScrollDirection.Horizontal:
                            needNewItem = m_ViewBounds.min.x > m_ContentBounds.min.x - 1;
                            break;
                        case ScrollDirection.Vertical:
                            needNewItem = m_ViewBounds.min.y < m_ContentBounds.min.y + 1;
                            break;
                    }

                    if (needNewItem)
                    {
                        NewItemAtEnd();
                    }
                }
            }
        }

        public void Change(int idx)
        {
            if (idx >= totalCount)
                return;
            if (Application.isPlaying)
            {
                if (idx >= m_StartIndex && idx < m_EndIndex)
                {
                    int index = idx - m_StartIndex;
                    if (itemShowHandler != null)
                    {
                        ScrollItem rectTrans = GetContentChild(index);
                        itemShowHandler(rectTrans, idx);
                    }
                }
            }
        }

        public void ShowItems(int count, int startidx = 0)
        {
            totalCount = count;
            RefillScrollItems(startidx);
        }

        public void Refresh(int idx)
        {
            if (idx >= totalCount)
                return;
            if (idx < m_StartIndex)
                idx = m_StartIndex;
            if (idx >= m_StartIndex && idx < m_EndIndex)
            {
                int index = idx - m_StartIndex;
                for (int i = index; i < content.childCount; i++)
                {
                    if (itemShowHandler != null)
                    {
                        ScrollItem rectTrans = GetContentChild(i);
                        itemShowHandler(rectTrans, idx);
                    }

                    idx++;
                }
            }
        }

        public void ClearCells()
        {
            if (Application.isPlaying)
            {
                m_StartIndex = 0;
                m_EndIndex = 0;
                totalCount = 0;
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    itemPool.RecyleItem<ScrollItem>(content.GetChild(i).gameObject);
                }
            }
        }

        #endregion


        #region ScrollRect Member

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2>
        {
        }

        [SerializeField] private RectTransform m_Content;

        public RectTransform content
        {
            get { return m_Content; }
            set { m_Content = value; }
        }

        [SerializeField] private ScrollDirection m_Direction;

        public ScrollDirection direction
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }

        [SerializeField] private MovementType m_MovementType = MovementType.Elastic;

        public MovementType movementType
        {
            get { return m_MovementType; }
            set { m_MovementType = value; }
        }

        [SerializeField] private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic

        public float elasticity
        {
            get { return m_Elasticity; }
            set { m_Elasticity = value; }
        }

        [SerializeField] private bool m_Inertia = true;

        public bool inertia
        {
            get { return m_Inertia; }
            set { m_Inertia = value; }
        }

        [SerializeField] private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

        public float decelerationRate
        {
            get { return m_DecelerationRate; }
            set { m_DecelerationRate = value; }
        }

        [SerializeField] private float m_ScrollSensitivity = 1.0f;

        public float scrollSensitivity
        {
            get { return m_ScrollSensitivity; }
            set { m_ScrollSensitivity = value; }
        }

        [SerializeField] private RectTransform m_Viewport;

        public RectTransform viewport
        {
            get { return m_Viewport; }
            set
            {
                m_Viewport = value;
                SetDirtyCaching();
            }
        }

        [SerializeField] private Scrollbar m_HorizontalScrollbar;

        public Scrollbar horizontalScrollbar
        {
            get { return m_HorizontalScrollbar; }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField] private Scrollbar m_VerticalScrollbar;

        public Scrollbar verticalScrollbar
        {
            get { return m_VerticalScrollbar; }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField] private ScrollbarVisibility m_HorizontalScrollbarVisibility;

        public ScrollbarVisibility horizontalScrollbarVisibility
        {
            get { return m_HorizontalScrollbarVisibility; }
            set
            {
                m_HorizontalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        [SerializeField] private ScrollbarVisibility m_VerticalScrollbarVisibility;

        public ScrollbarVisibility verticalScrollbarVisibility
        {
            get { return m_VerticalScrollbarVisibility; }
            set
            {
                m_VerticalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        [SerializeField] private float m_HorizontalScrollbarSpacing;

        public float horizontalScrollbarSpacing
        {
            get { return m_HorizontalScrollbarSpacing; }
            set
            {
                m_HorizontalScrollbarSpacing = value;
                SetDirty();
            }
        }

        [SerializeField] private float m_VerticalScrollbarSpacing;

        public float verticalScrollbarSpacing
        {
            get { return m_VerticalScrollbarSpacing; }
            set
            {
                m_VerticalScrollbarSpacing = value;
                SetDirty();
            }
        }

        [SerializeField] private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        public ScrollRectEvent onValueChanged
        {
            get { return m_OnValueChanged; }
            set { m_OnValueChanged = value; }
        }

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        protected Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        protected Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;

        public Vector2 velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }

        private bool m_Dragging;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        [NonSerialized] private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [System.NonSerialized] private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;

        #endregion

        protected LoopScrollView()
        {
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds(false);
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect =
                m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect =
                m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect &&
                              horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect &&
                              verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void Start()
        {
            base.Start();
            if (initInStart == false)
            {
                RefillScrollItems();
            }
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            delta.y *= -1;

            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                        delta.x = delta.y;
                    delta.y = 0;
                    break;
                case ScrollDirection.Vertical:
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        delta.y = delta.x;
                    delta.x = 0;
                    break;
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
                eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Dragging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive()) return;

            if (canDrag != null && !canDrag(eventData.delta))
            {
                m_PointerStartLocalCursor = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
                    eventData.pressEventCamera, out m_PointerStartLocalCursor);
                m_ContentStartPosition = m_Content.anchoredPosition;
                return;
            }

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
                    eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    position.y = m_Content.anchoredPosition.y;
                    break;
                case ScrollDirection.Vertical:
                    position.x = m_Content.anchoredPosition.x;
                    break;
            }

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected virtual void LateUpdate()
        {
            if (itemPool == null)
                return;
            if (!m_Content)
                return;
            if (initInStart == false)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                Vector2 position = m_Content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = m_Velocity[axis];
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis],
                            m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity,
                            deltaTime);
                        if (Mathf.Abs(speed) < 1)
                            speed = 0;
                        m_Velocity[axis] = speed;
                    }
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                        if (Mathf.Abs(m_Velocity[axis]) < 1)
                            m_Velocity[axis] = 0;
                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }

                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }

            if (m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds ||
                m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }

            UpdateScrollbarVisibility();
        }

        protected void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                    m_HorizontalScrollbar.size =
                        Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                    m_VerticalScrollbar.size =
                        Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get { return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition); }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set { SetNormalizedPosition(value, 0); }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;

                return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set { SetNormalizedPosition(value, 1); }
        }

        private void SetHorizontalNormalizedPosition(float value)
        {
            SetNormalizedPosition(value, 0);
        }

        private void SetVerticalNormalizedPosition(float value)
        {
            SetNormalizedPosition(value, 1);
        }

        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();

            float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];

            float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;

            float newLocalPosition =
                m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize *
                   Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }

        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual float minWidth
        {
            get { return -1; }
        }

        public virtual float preferredWidth
        {
            get { return -1; }
        }

        public virtual float flexibleWidth
        {
            get { return -1; }
        }

        public virtual float minHeight
        {
            get { return -1; }
        }

        public virtual float preferredHeight
        {
            get { return -1; }
        }

        public virtual float flexibleHeight
        {
            get { return -1; }
        }

        public virtual int layoutPriority
        {
            get { return -1; }
        }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta =
                    new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(vScrollingNeeded, direction == ScrollDirection.Vertical,
                m_VerticalScrollbarVisibility, m_VerticalScrollbar);
            UpdateOneScrollbarVisibility(hScrollingNeeded, direction == ScrollDirection.Horizontal,
                m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled,
            ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (scrollbarVisibility == ScrollbarVisibility.Permanent)
                {
                    if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                        scrollbar.gameObject.SetActive(xAxisEnabled);
                }
                else
                {
                    if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                        scrollbar.gameObject.SetActive(xScrollingNeeded);
                }
            }
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition =
                    new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing),
                        m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x,
                        -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        protected void UpdateBounds(bool updateItem = true)
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null) return;

            if (Application.isPlaying && updateItem && UpdateItems(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = GetBounds();
            }

            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            var contentPivot = m_Content.pivot;
            AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;

            if (movementType == MovementType.Clamped)
            {
                Vector2 delta = Vector2.zero;
                if (m_ViewBounds.max.x > m_ContentBounds.max.x)
                {
                    delta.x = System.Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x,
                        m_ViewBounds.max.x - m_ContentBounds.max.x);
                }
                else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
                {
                    delta.x = System.Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x,
                        m_ViewBounds.max.x - m_ContentBounds.max.x);
                }

                if (m_ViewBounds.min.y < m_ContentBounds.min.y)
                {
                    delta.y = System.Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y,
                        m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
                {
                    delta.y = System.Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y,
                        m_ViewBounds.max.y - m_ContentBounds.max.y);
                }

                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = m_Content.anchoredPosition + delta;
                    switch (direction)
                    {
                        case ScrollDirection.Horizontal:
                            contentPos.y = m_Content.anchoredPosition.y;
                            break;
                        case ScrollDirection.Vertical:
                            contentPos.x = m_Content.anchoredPosition.x;
                            break;
                    }

                    AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize,
            ref Vector3 contentPos)
        {
            Vector3 excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }

            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];

        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();
            m_Content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, direction, m_MovementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds,
            ScrollDirection direction, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            switch (direction)
            {
                case ScrollDirection.Horizontal:
                    min.x += delta.x;
                    max.x += delta.x;
                    if (min.x > viewBounds.min.x)
                        offset.x = viewBounds.min.x - min.x;
                    else if (max.x < viewBounds.max.x)
                        offset.x = viewBounds.max.x - max.x;
                    break;
                case ScrollDirection.Vertical:
                    min.y += delta.y;
                    max.y += delta.y;
                    if (max.y < viewBounds.max.y)
                        offset.y = viewBounds.max.y - max.y;
                    else if (min.y > viewBounds.min.y)
                        offset.y = viewBounds.min.y - min.y;
                    break;
            }

            return offset;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirtyCaching();
    }

#endif
    }
}