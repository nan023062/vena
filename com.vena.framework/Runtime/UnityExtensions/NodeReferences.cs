// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Vena
{
    public class NodeReferences : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField]
        public UnityEngine.GameObject[] nodeRefs;
        
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
        }

        [ContextMenu("检查 NodeRefs")]
        private void CheckNodeRef()
        {
            bool result = true;
            if(nodeRefs != null)
            {
                List<string> lst = new List<string>();
                foreach (var nodeRef in nodeRefs)
                {
                    if (nodeRef == null)
                    {
                        Debug.Error($"{name}存在空节点!");
                        result = false;
                        continue;
                    }
                    if (lst.Contains(nodeRef.name))
                    {
                        Debug.Error($"{name}存在重复节点: name = {nodeRef.name}!");
                        result = false;
                    }
                    else
                    {
                        lst.Add(nodeRef.name);
                    }
                }
            }
            if (result) Debug.Log($"{name} 的UINodeRefs 格式正确!");
        }
#endif
        
        public T GetRef<T>(string name) where T : UnityEngine.Component
        {
            foreach (GameObject go in nodeRefs)
            {
                if (go.name.Equals(name))
                    return go.GetComponent<T>();
            }
            return null;
        }
        
        public GameObject GetRef(string name)
        {
            foreach (GameObject go in nodeRefs)
            {
                if (go.name.Equals(name))
                    return go;
            }
            return null;
        }
        
        // public T GetRef<T>(string name) where T : UnityEngine.Object
        // {
        //     foreach (var nodeRef in nodeRefs)
        //     {
        //         if (nodeRef.name.Equals(name))
        //         {
        //             return nodeRef as T;
        //         }
        //     }
        //     return null;
        // }
    }
}