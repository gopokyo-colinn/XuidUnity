using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// GroupElement class.
    /// based on Baum2.Editor.GroupElement class.
    /// </summary>
    public class GroupElement : Element
    {
        private Area _areaCache;
        protected readonly Dictionary<string, object> CanvasGroup;
        protected readonly Dictionary<string, object> LayoutJson;
        protected readonly Dictionary<string, object> ContentSizeFitterJson;
        protected readonly Dictionary<string, object> MaskJson;
        protected bool? RectMask2D;
        protected readonly string FillColorJson;
        protected Dictionary<string, object> AddComponentJson;
        protected readonly List<object> ComponentsJson;

        // children
        protected readonly List<Element> Elements;

        public GroupElement(Dictionary<string, object> json, Element parent, bool resetStretch = false) : base(json,
            parent)
        {
            Elements = new List<Element>();
            var jsonElements = json.Get<List<object>>("elements");
            foreach (var jsonElement in jsonElements)
            {
                var elem = ElementFactory.Generate(jsonElement as Dictionary<string, object>, this);
                if (elem != null)
                    Elements.Add(elem);
            }

            Elements.Reverse();
            CanvasGroup = json.GetDic("canvas_group");
            LayoutJson = json.GetDic("layout");
            ContentSizeFitterJson = json.GetDic("content_size_fitter");
            MaskJson = json.GetDic("mask");
            RectMask2D = json.GetBool("rect_mask_2d");
            FillColorJson = json.Get("fill_color");
            AddComponentJson = json.GetDic("add_component");
            ComponentsJson = json.Get<List<object>>("components");
        }


        public List<Tuple<GameObject, Element>> RenderedChildren { get; private set; }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateSelf(renderContext);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            RenderedChildren = RenderChildren(renderContext, go);
            ElementUtil.SetupCanvasGroup(go, CanvasGroup);
            ElementUtil.SetupChildImageComponent(go, RenderedChildren);
            ElementUtil.SetupFillColor(go, FillColorJson);
            ElementUtil.SetupContentSizeFitter(go, ContentSizeFitterJson);
            ElementUtil.SetupLayoutGroup(go, LayoutJson);
            ElementUtil.SetupLayoutElement(go, LayoutElementJson);
            ElementUtil.SetupComponents(go, ComponentsJson);
            ElementUtil.SetupMask(go, MaskJson);

            ElementUtil.SetupRectTransform(go, RectTransformJson);

            return go;
        }


        protected virtual GameObject CreateSelf(RenderContext renderContext)
        {
            var go = CreateUiGameObject(renderContext);
            return go;
        }

        protected void SetMaskImage(RenderContext renderContext, GameObject go)
        {
            var maskSource = Elements.Find(x => x is MaskElement);
            if (maskSource == null) return;

            Elements.Remove(maskSource);
            var maskImage = go.AddComponent<Image>();
            maskImage.raycastTarget = false;

            var dummyMaskImage = maskSource.Render(renderContext, null);
            dummyMaskImage.transform.SetParent(go.transform);
            dummyMaskImage.GetComponent<Image>().CopyTo(maskImage);
            Object.DestroyImmediate(dummyMaskImage);

            var mask = go.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }

        protected List<Tuple<GameObject, Element>> RenderChildren(RenderContext renderContext, GameObject parent,
            Action<GameObject, Element> callback = null)
        {
            var list = new List<Tuple<GameObject, Element>>();
            foreach (var element in Elements)
            {
                var go = element.Render(renderContext, parent);
                if (go.transform.parent != parent.transform)
                {
                    Debug.Log("親が設定されていない" + go.name);
                }

                list.Add(new Tuple<GameObject, Element>(go, element));
                if (callback != null)
                {
                    callback.Invoke(go, element);
                }
            }

            foreach (var element in Elements)
            {
                element.RenderPass2(list);
            }

            return list;
        }

    }
}