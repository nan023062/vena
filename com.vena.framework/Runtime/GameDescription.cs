//****************************************************************************
// File: GameDescription.cs
// Author: Li Nan
// Date: 2023-11-25 12:00
// Version: 1.0
//****************************************************************************
using System.Reflection;
using UnityEngine;

namespace Vena.Framework
{
    /// <summary>
    /// game description
    /// </summary>
    public readonly struct GameDescription
    {
        public readonly Camera MainCamera;
        public readonly UIRoot UIRoot;
        public readonly IUILoader UILoader;
        public readonly ISceneLoader SceneLoader;
        public readonly Assembly[] Assemblies;

        public GameDescription(Camera mainCamera, UIRoot uiRoot,
            IUILoader uiLoader, ISceneLoader sceneLoader, Assembly[] assemblies)
        {
            MainCamera = mainCamera;
            UIRoot = uiRoot;
            UILoader = uiLoader;
            SceneLoader = sceneLoader;
            Assemblies = assemblies;
        }
    }
}
