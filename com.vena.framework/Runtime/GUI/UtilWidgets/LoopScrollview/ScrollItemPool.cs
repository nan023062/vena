using System.Collections.Generic;
using UnityEngine;

namespace Vena.Framework
{
    public class ScrollItemPool : MonoBehaviour
    {
        public int maxCount = 50;
        public ScrollItem cell;
        private Queue<ScrollItem> m_Queue = new Queue<ScrollItem>();

        public ScrollItem[] childTemps;
        private Dictionary<int, Queue<ScrollItem>> m_ChildQueue = new Dictionary<int, Queue<ScrollItem>>();

        private Transform m_CacheTransform;

        void Awake()
        {
            m_CacheTransform = transform;
        }

        public T FetchItem<T>() where T : ScrollItem
        {
            T rt = null;
            if (m_Queue.Count > 0)
            {
                rt = m_Queue.Dequeue() as T;
                rt.Reset();
            }
            else
            {
                GameObject itemgo = InstantiateItem();
                if (itemgo != null)
                {
                    rt = itemgo.GetComponent<T>();
                    itemgo.SetActive(true);
                    InitCell(rt);
                }
            }

            rt.cacheRectTransform.localScale = Vector3.one;
            return rt;
        }

        public void RecyleItem<T>(GameObject go) where T : ScrollItem
        {
            T rt = go.GetComponent<T>();
            rt.Reset();
            if (m_Queue.Count < maxCount - 1)
            {
                m_Queue.Enqueue(rt);
                go.SetActive(false);
                rt.cacheTransform.SetParent(m_CacheTransform);
            }
            else
            {
                Destroy(go);
            }
        }

        private GameObject InstantiateItem()
        {
            if (cell != null)
            {
                GameObject item = Instantiate(cell.gameObject);
                return item;
            }

            return null;
        }

        public T FetchChild<T>(int index) where T : ScrollItem
        {
            T rt = null;
            Queue<ScrollItem> queue = GetQueueByType(index);
            if (queue.Count > 0)
            {
                rt = queue.Dequeue() as T;
                rt.Reset();
            }
            else
            {
                GameObject itemgo = InstantiateChild(index);
                if (itemgo != null)
                {
                    rt = itemgo.GetComponent<T>();
                    itemgo.SetActive(true);
                    InitCell(rt);
                }
            }

            rt.cacheRectTransform.localScale = Vector3.one;
            return rt;
        }

        public void PutChild<T>(T rt, int index) where T : ScrollItem
        {
            if (rt == null)
                return;
            Queue<ScrollItem> queue = GetQueueByType(index);
            rt.Reset();
            if (queue.Count < maxCount - 1)
            {
                queue.Enqueue(rt);
                rt.cacheObejct.SetActive(false);
                rt.cacheTransform.SetParent(m_CacheTransform);
            }
            else
            {
                Destroy(rt.cacheObejct);
            }
        }

        private void ClearPool()
        {
            cell = null;
            childTemps = null;
            m_Queue.Clear();
            m_ChildQueue.Clear();
        }

        private GameObject InstantiateChild(int index)
        {
            if (childTemps.Length > index && childTemps[index] != null)
            {
                GameObject item = Instantiate(childTemps[index].gameObject);
                return item;
            }

            return null;
        }

        private Queue<ScrollItem> GetQueueByType(int index)
        {
            Queue<ScrollItem> queue;
            if (!m_ChildQueue.TryGetValue(index, out queue))
            {
                queue = new Queue<ScrollItem>();
                m_ChildQueue.Add(index, queue);
            }

            return queue;
        }

        private void InitCell(ScrollItem item)
        {
            item.pool = this;
        }
    }
}