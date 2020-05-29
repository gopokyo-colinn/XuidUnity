using System;
using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    ///     Element class.
    ///     based on Baum2.Editor.Element class.
    /// </summary>
    public abstract class Element
    {
        protected readonly string Layer;
        protected readonly Dictionary<string, object> LayoutElementJson;
        protected readonly List<object> ParsedNames;

        protected readonly Dictionary<string, object> RectTransformJson;
        protected bool? Active;
        protected string name;
        protected Element Parent;

        protected Element(Dictionary<string, object> json, Element parent)
        {
            Parent = parent;
            name = json.Get("name");
            // Debug.Log($"parsing {name}");
            Active = json.GetBool("active");
            Layer = json.Get("layer");
            ParsedNames = json.Get<List<object>>("parsed_names");

            RectTransformJson = json.GetDic("rect_transform");
            LayoutElementJson = json.GetDic("layout_element");
        }

        public string Name => name;

        public abstract GameObject Render(RenderContext renderContext, GameObject parentObject);

        public virtual void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
        }

        public bool HasParsedName(string parsedName)
        {
            if (ParsedNames == null || ParsedNames.Count == 0) return false;
            var found = ParsedNames.Find(s => (string) s == parsedName);
            return found != null;
        }

        protected GameObject CreateUiGameObject(RenderContext renderContext)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            ElementUtil.SetLayer(go, Layer);
            if (Active != null) go.SetActive(Active.Value);

            return go;
        }
    }
}