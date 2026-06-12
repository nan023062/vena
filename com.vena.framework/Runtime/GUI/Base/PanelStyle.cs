// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Framework
{
    public enum PanelStyle
    {
        Page = 0,               //全屏页面
        FixWindow = 1,          //固定窗口
        PopWindow = 2,          //弹窗窗口
        Tutorial = 3,           //指引界面
        //以上是有UI交互
        
        //以下是无UI交互--不会与场景一起关闭
        GameTransit = 4,        //过场界面
        BoardCast = 5,          //广播界面
        UIToolTips = 6,         //提示界面
    }
}