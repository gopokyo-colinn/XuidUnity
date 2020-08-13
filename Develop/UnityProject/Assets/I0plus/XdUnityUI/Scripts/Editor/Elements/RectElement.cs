using System.Collections.Generic;
using UnityEngine;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     RectElement class.
    /// </summary>
    public sealed class RectElement : Element
    {
        public RectElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        public override void Render(ref GameObject selfObject, RenderContext renderContext, GameObject parentObject)
        {
            GetOrCreateSelfObject(renderContext, ref selfObject, parentObject);
            var rect = selfObject.GetComponent<RectTransform>();
            if (parentObject)
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);

            ElementUtil.SetupRectTransform(selfObject, RectTransformJson);
        }
    }
}