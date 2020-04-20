using System;
using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// Element class.
    /// based on Baum2.Editor.Element class.
    /// </summary>
    public abstract class Element
    {
        public string name;
        protected bool? active;
        protected string layer;
        protected List<object> parsedNames;
        protected Element parent;

        protected Dictionary<string, object> rectTransformJson;
        protected Dictionary<string, object> LayoutElementParam;

        public abstract GameObject Render(RenderContext renderContext, GameObject parentObject);

        public virtual void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
        }

        public abstract Area CalcArea();

        protected Element(Dictionary<string, object> json, Element parent)
        {
            this.parent = parent;
            name = json.Get("name");
            active = json.GetBool("active");
            layer = json.Get("layer");
            parsedNames = json.Get<List<object>>("parsed_names");

            rectTransformJson = json.GetDic("rect_transform");
            LayoutElementParam = json.GetDic("layout_element");
        }

        public bool HasParsedName(string parsedName)
        {
            if (parsedNames == null || parsedNames.Count == 0) return false;
            var found = parsedNames.Find(s => (string) s == parsedName);
            return found != null;
        }

        protected GameObject CreateUIGameObject(RenderContext renderContext)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            ElementUtil.SetLayer(go, layer);
            if (active != null)
            {
                go.SetActive(active.Value);
            }

            return go;
        }
    }
}