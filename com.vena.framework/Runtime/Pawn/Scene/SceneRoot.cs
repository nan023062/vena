// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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