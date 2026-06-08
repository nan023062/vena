//****************************************************************************
// File: PanelAttribute.cs
// Author: Li Nan
// Date: 2022-12-18 12:00
// Version: 1.0
//****************************************************************************

using System;

namespace Vena.Framework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class PanelAttribute : Attribute
    {
        public readonly PanelStyle style;          //UI风格
        
        public readonly bool raycastBlock;          //UI事件遮罩

        public readonly bool allowCache;            //可缓存
        
        public readonly string resName;             //资源名
        
        public PanelAttribute(PanelStyle style, bool raycastBlock, bool allowCache, string resName)
        {
            this.style = style;
            
            this.raycastBlock = raycastBlock;
            
            this.allowCache = allowCache;
             
            this.resName = resName;
        }
    }
}

