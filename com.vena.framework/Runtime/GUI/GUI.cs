using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Vena.Framework
{
    public interface IUILoader
    {
        GameObject LoadPanelGameObject(Type type);
        
        void UnloadPanelGameObject(Type type);
    }

    // game gui manager
    public partial class GameWorld
    {
        private static UIRoot _UIRoot;
        private static IUILoader _UILoader;
        public static readonly int MaxCachePanel = 5;
        private static readonly Dictionary<PanelStyle, PanelGroup<Type>> styleGroups = new Dictionary<PanelStyle, PanelGroup<Type>>();
        private static readonly Dictionary<Type, PanelData> panels = new Dictionary<Type, PanelData>();
        private static readonly Dictionary<Type, PanelData> cached = new Dictionary<Type, PanelData>();
        
        #region Panel Manager
        
        class PanelData
        {
            public GuiPanel panel;
            public object arg;                  //动态参数
            public uint times;                  //开启次数--用于计算开启频率
        }
        
        public static GuiPanel OpenPanel(Type type, object arg = null)
        {
            if (panels.TryGetValue(type, out var panelData))
            {
                Debug.Error($"the opening panel = { type.Name} has opened !");
                return panelData.panel;
            }

            var config = type.GetCustomAttribute<PanelAttribute>();
            if(null != config)
            {
                var panelGroup = GetPanelGroup(config.style);
                
                //全屏界面，堆栈管理
                if (panelGroup.Style == PanelStyle.Page)
                {
                    //压入栈，且直接关闭当前全屏页面
                    Type topType = panelGroup.Top();
                    if (null != topType && panels.TryGetValue(topType, out var topPanelData))
                    {
                        //关闭所有窗口界面
                        CloseStylePanel(PanelStyle.FixWindow);
                        DestroyPanel(topPanelData);
                    }
                }
                
                if (cached.TryGetValue(type, out panelData))
                {
                    cached.Remove(type);
                }
                else
                {
                    var objectPanel = CreatePanel(type);
                    panelData = new PanelData
                    {
                        panel = objectPanel,
                        times = 0,
                    };
                }
                
                panels.Add(type, panelData);
                panelData.arg = arg;
                
                int baseDepth = (int)config.style * 1000;
                if (config.style != PanelStyle.Page && panelGroup.Top() != null)
                {
                    var topPanelData = panels[panelGroup.Top()];
                    panelData.panel.depth = topPanelData.panel.maxDepth + 10;
                }
                else
                {
                    panelData.panel.depth = baseDepth;
                }
                
                panelData.panel.Enable(arg);
                panelData.panel.panel.canvasGroup.alpha = 1f;
                
                panelGroup.Push(type);
                panelData.times++;
                
                return panelData.panel;
            }
            
            Debug.Error($"not find [{type.FullName}] config!");
            return null;
        }

        public static T OpenPanel<T>(params object[] args) where T : GuiPanel
        {
            return OpenPanel(typeof(T), args) as T;
        }

        private static GuiPanel CreatePanel(Type type)
        {
            GameObject go = _UILoader.LoadPanelGameObject(type);
            if (World.Default.CreateActor(type) is GuiPanel panel)
            {
                ((IGui)panel).Init(go);
                panel.panel.canvasGroup.alpha = 0;
                var rectTransform = panel.transform;
                
                Transform normalCanvas = _UIRoot.canvas_normal.transform;
                rectTransform.SetParent(normalCanvas);
                    
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
                    
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                return panel;
            }

            throw new Exception($"CreatePanel( {type} ) failed !!");
        }
        
        public static void ClosePanel<T>() where T : GuiPanel
        {
            Type type = typeof(T);

            if (panels.TryGetValue(type, out _))
            {
                ClosePanel(type);
            }
            else
            {
                Debug.Error($"the closing panel = {type.FullName} not exits ！");
            }
        }

        public static void ClosePanel(Type panelType)
        {
            if (!panels.TryGetValue(panelType, out var panelData))
            {
                Debug.Error($"the closing panel = {panelType.FullName} not exits ！");
                return;
            }
            
            var objectPanel = panelData.panel;
            
            PanelGroup<Type> panelList = GetPanelGroup(objectPanel.style);

            if (objectPanel.style == PanelStyle.Page)
            {
                if (panelList.Count > 1)
                {
                    var topType = panelList.Pop();
                    if (topType != panelType)
                    {
                        Debug.Warning($"closing panel = {panelType.FullName} not top of panel stack ！ style = {objectPanel.style}");
                    }
                    if (!panels.TryGetValue(topType, out var topPanelRef))
                    {
                        Debug.Error($"top panel is {topType.FullName} not exits！ style = {objectPanel.style}");
                        return;
                    }
                        
                    CloseStylePanel(PanelStyle.FixWindow);
                    DestroyPanel(panelData);
                    OpenPanel(topType, topPanelRef.arg);
                }
                else
                {
                    Debug.Warning("don't close the last page !");
                }
            }
            else
            {
                var topType = panelList.Pop();
                if (topType != panelType)
                {
                    Debug.Error($"closed not top of panel stack ！ style = {objectPanel.style}");
                }
                DestroyPanel(panelData);
            }
        }

        private static void DestroyPanel(PanelData panelData)
        {
            panelData.panel.panel.canvasGroup.alpha = 0;
            panelData.panel.Disable();
            
            Type type = panelData.panel.GetType();
            
            if (panels.Remove(type))
            {
                var config = type.GetCustomAttribute<PanelAttribute>();
                if (config.allowCache)
                {
                    cached[type] = panelData;
                    panelData = null;
                    
                    //已经缓存满了
                    if (cached.Count > MaxCachePanel)
                    {
                        uint times = uint.MaxValue;
                        foreach (var keyValue in cached)
                        {
                            if (keyValue.Value.times < times)
                            {
                                panelData = keyValue.Value;
                                times = keyValue.Value.times;
                            }
                        }
                    }
                }
            }
            
            if (panelData != null)
            {
                Type destroyType = panelData.panel.GetType();
                cached.Remove(destroyType);
                _UILoader.UnloadPanelGameObject(destroyType);
                panelData.panel.Destroy();
            }
        }

        private static void CloseStylePanel(PanelStyle style)
        {
            var panelGroup = GetPanelGroup(style);
            while (panelGroup.Count > 0)
            {
                if (panels.TryGetValue(panelGroup.Pop(), out var panel))
                {
                    DestroyPanel(panel);
                }
            }
        }

        public static void CloseAllPanel(bool includeOverlay = false)
        {
            foreach (var panelData in panels.Values.ToArray())
            {
                if (includeOverlay || panelData.panel.style <= PanelStyle.Tutorial)
                {
                    styleGroups.Remove(panelData.panel.style);
                    
                    DestroyPanel(panelData);
                }
            }
            
            // clear cache panels
            PanelData[] panelDataArray = cached.Values.ToArray();
            cached.Clear();
            foreach (var panelData in panelDataArray)
            {
                if (panelData != null)
                {
                    panelData.panel.Destroy();
                }
            }
        }

        private static PanelGroup<Type> GetPanelGroup(PanelStyle style)
        {
            if (!styleGroups.TryGetValue(style, out var panelGroup))
            {
                panelGroup = new PanelGroup<Type>(style);
                
                styleGroups.Add(style, panelGroup);
            }
            
            return panelGroup;
        }

        #endregion
    }
}
