using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vena.Framework
{
    public class UIOutlineEx : BaseMeshEffect
    {
        public Color outlineColor1 = Color.white;
        public Color outlineColor2 = Color.red;

        [Range(1, 10)] public int outlineWidth1 = 1;
        [Range(1, 10)] public int outlineWidth2 = 1;

        private static List<UIVertex> s_vetexList = new List<UIVertex>();
        private static Shader s_outlineShader = null;
        private Vector2 _MainTex_TexelSize = Vector2.one;
        private Vector4 _rect = new Vector4(1, 1, 0, 0);

        protected override void Awake()
        {
            base.Awake();

            if (s_outlineShader == null)
            {
                s_outlineShader = Shader.Find("FTF/UI/UIOutlineEx_Sprite");
            }

            base.graphic.material = new Material(s_outlineShader);
            base.graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
            Refresh();
        }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (graphic.material != null) Refresh();
    }

#endif

        private void Refresh()
        {
            _MainTex_TexelSize.x = 1f / graphic.mainTexture.width;
            _MainTex_TexelSize.y = 1f / graphic.mainTexture.height;

            graphic.material.SetColor("_OutlineColor1", outlineColor1);
            graphic.material.SetColor("_OutlineColor2", outlineColor2);

            graphic.material.SetFloat("_OutlineWidth1", (float)outlineWidth1);
            graphic.material.SetFloat("_OutlineWidth2", (float)outlineWidth2);

            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            vh.GetUIVertexStream(s_vetexList);

            ProcessVertices();

            vh.Clear();
            vh.AddUIVertexTriangleStream(s_vetexList);
        }

        /// <summary>
        /// 处理顶点的UV
        /// 根据描边宽度进行修正，确保描边不会被裁减
        /// </summary>
        private void ProcessVertices()
        {
            int count = s_vetexList.Count;

            Vector4 rect = _rect;
            for (int i = 0; i < count; i++)
            {
                Vector2 uv = s_vetexList[i].uv0;
                rect.x = Mathf.Min(rect.x, uv.x);
                rect.y = Mathf.Min(rect.y, uv.y);
                rect.z = Mathf.Max(rect.z, uv.x);
                rect.w = Mathf.Max(rect.w, uv.y);
            }

            Vector2 center = new Vector2(rect.x + rect.z, rect.y + rect.w) * 0.5f;
            Vector2 size = new Vector2(rect.z - rect.x, rect.w - rect.y);
            Vector2 scaler = _MainTex_TexelSize * (outlineWidth1 + outlineWidth2) * 2f + Vector2.one;

            for (int i = 0; i < count; i++)
            {
                UIVertex vertex = s_vetexList[i];

                Vector2 delta = new Vector2(vertex.uv0.x,vertex.uv0.y) - center;
                delta.x *= scaler.x;
                delta.y *= scaler.y;

                vertex.uv0 = delta + center;
                s_vetexList[i] = vertex;
            }
        }
    }
}