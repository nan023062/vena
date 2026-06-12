// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vena.Framework
{
    public delegate void OnRefreshDropItem(int index, CustomDropItem item);
    
    public delegate void OnRefreshDropdown(int index);
    
    [RequireComponent(typeof(RectTransform))]
    public class CustomDropdown : UIElement
    {
        public Button dropdownButton;
        public CustomDropItem templeteItem;
        public ScrollRect dropdownRect;
        public Transform dropdownArrow;
        public float tweenDuration = 0.2f;
        public float dropItemSize = 30f;
        private RectTransform mContent;
        private OnRefreshDropItem mRefreshItemEvent;
        private OnRefreshDropdown mRefreshDropdownEvent;
        private int mCount;
        private int mCheckIndex;

        public void SetLocked( bool locked)
        {
            if (dropdownButton != null)
            {
                dropdownButton.enabled = !locked;
            }
        }

        private List<CustomDropItem> mDropItemLst;

        private void OnClickedDropdown()
        {
            Transform dropdownTrans = dropdownRect.transform;
            Vector3 dropScale = dropdownTrans.localScale;
            
            if (!dropdownRect.gameObject.activeSelf)
            {
                dropdownRect.gameObject.SetActive(true);
            }

            //展开
            if (dropScale.y <= 0.01f)
            {
                RefreshDropdown();
                Quaternion rotate = Quaternion.Euler(new Vector3(0,0,180));
                //dropdownArrow.DOLocalRotateQuaternion(rotate, tweenDuration);
                //dropdowTrans.DOScaleY(1, tweenDuration).SetEase(Ease.OutBack);
            }
            //收回
            else
            {
                Quaternion rotate = Quaternion.Euler(new Vector3(0, 0, 0));
                //dropdownArrow.DOLocalRotateQuaternion(rotate, tweenDuration);
                //dropdowTrans.DOScaleY(0, tweenDuration).SetEase(Ease.InBack);
            }
        }

        private void RefreshDropdown()
        {
            if (mDropItemLst == null) return;

            for (int i = mDropItemLst.Count; i < mCount; i++)
            {
                GameObject go = Instantiate(templeteItem.gameObject);
                CustomDropItem item = go.GetComponent<CustomDropItem>();
                var rect = item.transform as RectTransform;
                rect.SetParent(mContent.transform);
                rect.offsetMax = Vector2.one * 0.5f;
                rect.offsetMin = Vector2.one * 0.5f;
                rect.pivot = Vector2.one * 0.5f;
                rect.sizeDelta = (templeteItem.transform as RectTransform).sizeDelta;
                rect.localScale = (templeteItem.transform as RectTransform).localScale;
                item.onClick -= OnClickDropItem;
                item.onClick += OnClickDropItem;
                mDropItemLst.Add(item);
            }

            RectTransform dropRect = dropdownRect.transform as RectTransform;
            float heigh = mCount * dropItemSize;
            dropRect.sizeDelta = new Vector2(dropRect.sizeDelta.x, heigh);

            for (int i = 0; i < mDropItemLst.Count; i++)
            {
                CustomDropItem item = mDropItemLst[i];
                item.gameObject.SetActive(i < mCount);
                item.check.SetActive(i == mCheckIndex);
                var rectTrans = item.transform as RectTransform;
                rectTrans.anchoredPosition = Vector2.down * ((i + 0.5f) * dropItemSize - 0.5f * heigh);
                if (i < mCount)
                {
                    if (mRefreshItemEvent != null)
                        mRefreshItemEvent(i, item);
                }
            }
        }

        private void OnClickDropItem(CustomDropItem item)
        {
            for (int i = 0; i < mDropItemLst.Count; i++)
            {
                CustomDropItem __item = mDropItemLst[i];
                if (item == __item) mCheckIndex = i;
                if (i < mCount) __item.check.SetActive(item == __item);
            }
            //dropdownRect.gameObject.SetActive(false);
            OnClickedDropdown();
            if (mRefreshDropdownEvent != null) mRefreshDropdownEvent(mCheckIndex);
        }

        protected override void Awake()
        {
            base.Awake();
            InitState();
        }

        private void InitState()
        {
            mCheckIndex = 0;
            dropdownButton.gameObject.SetActive(true);
            templeteItem.gameObject.SetActive(false);
            dropdownRect.gameObject.SetActive(true);
            dropdownRect.transform.localScale = new Vector3(1, 0, 1);
            dropdownButton.onClick.RemoveAllListeners();
            dropdownButton.onClick.AddListener(OnClickedDropdown);
            if (mDropItemLst == null) mDropItemLst = new List<CustomDropItem>();
            mContent = templeteItem.transform.parent as RectTransform;         
        }

        public void Init(int count, OnRefreshDropdown refreshDropdown, OnRefreshDropItem refreshDropItem)
        {
            InitState();
            this.mCount = count;
            mRefreshItemEvent = refreshDropItem;
            mRefreshDropdownEvent = refreshDropdown;
            if (mRefreshDropdownEvent != null) mRefreshDropdownEvent(mCheckIndex);
            RefreshDropdown();
        }

        public void SetDefault(int index)
        {
            index = Mathf.Min(index, mCount);
            index = Mathf.Max(index, 0);
            mCheckIndex = index;
            if (mRefreshDropdownEvent != null) mRefreshDropdownEvent(index);
        }

#if UNITY_EDITOR
        [ContextMenu("添加4个自己对象")]
        private void Test()
        {
            Init(4, null, null);
        }
#endif
    }
}
