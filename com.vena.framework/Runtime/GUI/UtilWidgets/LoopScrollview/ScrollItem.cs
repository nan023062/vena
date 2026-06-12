// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    public class ScrollItem : MonoBehaviour
    {
        [HideInInspector] public ScrollItemPool pool;
        [HideInInspector] public List<ScrollItem> childcells = new List<ScrollItem>();

        [HideInInspector] public RectTransform cacheRectTransform;
        [HideInInspector] public GameObject cacheObejct;
        [HideInInspector] public Transform cacheTransform;
        [HideInInspector] public LayoutElement cacheLayoutElement;
        [HideInInspector] public int childindex = -1;

        void Awake()
        {
            cacheRectTransform = GetComponent<RectTransform>();
            cacheLayoutElement = GetComponent<LayoutElement>();
            cacheObejct = gameObject;
            cacheTransform = transform;
        }

        public void Reset()
        {
            if (childcells.Count > 0)
            {
                foreach (var item in childcells)
                {
                    item.Reset();
                    pool.PutChild(item, item.childindex);
                }
            }

            childcells.Clear();
        }

        public T GetChild<T>(int index) where T : ScrollItem
        {
            var childcell = pool.FetchChild<T>(index);
            childcell.childindex = index;
            childcell.cacheObejct.SetActive(true);
            childcell.cacheRectTransform.anchorMax = Vector2.up;
            childcell.cacheRectTransform.anchorMin = Vector2.up;
            childcell.cacheRectTransform.pivot = Vector2.up;
            childcell.cacheRectTransform.anchoredPosition = Vector2.zero;
            childcell.cacheRectTransform.localScale = Vector3.one;
            childcell.cacheTransform.SetParent(cacheTransform, false);
            childcells.Add(childcell);
            return childcell as T;
        }
    }
}