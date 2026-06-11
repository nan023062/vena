// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    [RequireComponent(typeof(Graphic))]
    public class UIPentagon : MonoBehaviour
    {
        public Vector2[] Point = new Vector2[num - 1];
        private Graphic mGraphic;
        private Vector4[] mTempPoints;
        private const int num = 6;

        public void Awake()
        {
            mGraphic = GetComponent<Graphic>();
            mTempPoints = new Vector4[num];
        }

        public void UpdatePoints()
        {
            if (Point.Length < num - 1)
            {
                Debug.Error("UIPentagon的顶点数不足5个！");
                return;
            }

            for (int i = 0; i < num - 1; i++)
            {
                mTempPoints[i] = Point[i];
            }

            mTempPoints[num - 1] = Point[0];

            if (mGraphic.material.HasProperty("_Points"))
            {
                mGraphic.material.SetVectorArray("_Points", mTempPoints);
            }
            else
            {
                Debug.Error("UIPentagon的的材质球没有换成 Game/UI/FTF_UIPentagonShader ...");
            }
        }

#if UNITY_EDITOR
    private void Update()
    {
        UpdatePoints();
    }
#endif
    }
}