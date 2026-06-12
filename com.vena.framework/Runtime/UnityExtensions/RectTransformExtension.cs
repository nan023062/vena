// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena
{
    public static class RectTransformExtension
    {
        public static void SpreadFormat(this RectTransform rectTransform)
        {
        


        }

        public static void SpreadFormat(this RectTransform rectTransform, Transform transform)
        {
            rectTransform.SetParent(transform);
            rectTransform.SpreadFormat();
        }
    }
}
