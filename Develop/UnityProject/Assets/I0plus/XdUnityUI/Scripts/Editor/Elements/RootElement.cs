using System.Collections.Generic;
using UnityEngine;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     RootElement class.
    ///     based on Baum2.Editor.RootElement class.
    /// </summary>
    public class RootElement : GroupElement
    {
        public RootElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        protected override GameObject CreateSelf(RenderContext renderContext)
        {
            var go = CreateUiGameObject(renderContext);

            var rect = go.GetComponent<RectTransform>();
            ElementUtil.SetupRectTransform(go, RectTransformJson);
            ElementUtil.SetLayer(go, Layer);
            SetMaskImage(renderContext, go);
            return go;
        }
    }
}