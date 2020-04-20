using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// RootElement class.
    /// based on Baum2.Editor.RootElement class.
    /// </summary>
    public class RootElement : GroupElement
    {
        private Vector2 sizeDelta = Vector2.zero;

        public RootElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        protected override GameObject CreateSelf(RenderContext renderContext)
        {
            var go = CreateUIGameObject(renderContext);

            var rect = go.GetComponent<RectTransform>();
            ElementUtil.SetRectTransform(go, rectTransformJson);
            ElementUtil.SetLayer(go, layer);
            SetMaskImage(renderContext, go);
            return go;
        }

        public override Area CalcArea()
        {
            return new Area(-sizeDelta / 2.0f, sizeDelta / 2.0f);
        }
    }
}