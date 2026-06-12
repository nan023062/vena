// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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
