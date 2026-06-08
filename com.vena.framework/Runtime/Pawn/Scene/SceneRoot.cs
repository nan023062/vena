//****************************************************************************
// File: SceneRoot.cs
// Author: Li Nan
// Date: 2023-08-20 12:00
// Version: 1.0
//****************************************************************************

using UnityEngine;
using Vena.UnityExtensions;

namespace Vena.Framework
{
    public class SceneRoot : NodeReferences
    {
        public Material skybox;
        public LightingSettings lightingSettings;
        
        private void Awake()
        {
            RenderSettings.skybox = skybox;
        }
    }
}