using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// RectElement class.
    /// </summary>
    public sealed class RectElement : Element
    {
        public RectElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateUiGameObject(renderContext);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }
    }
}