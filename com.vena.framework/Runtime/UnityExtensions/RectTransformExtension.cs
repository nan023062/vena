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
