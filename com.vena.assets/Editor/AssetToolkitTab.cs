// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEditor;

namespace Vena.Assets
{
    /// <summary>
    /// editor sub gui view
    /// </summary>
    public abstract class AssetToolkitTab : EditorWindow
    {
        public abstract string TabName { get; }
        
        public void EnterDraw()
        {
            OnEnterDraw();
        }

        public void DrawGUI()
        {
            OnDrawGUI();
        }
        
        public void ExitDraw()
        {
            OnExitDraw();
        }

        protected abstract void OnDrawGUI();

        protected abstract void OnEnterDraw();

        protected abstract void OnExitDraw();
    }
}
