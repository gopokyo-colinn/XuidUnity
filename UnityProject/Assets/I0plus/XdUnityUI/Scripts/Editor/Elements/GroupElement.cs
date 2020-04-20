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
        protected readonly List<Element> Elements;
        private Area _areaCache;
        private Dictionary<string, object> _canvasGroupParam;
        protected Dictionary<string, object> LayoutParam;
        protected Dictionary<string, object> ContentSizeFitterParam;
        protected Dictionary<string, object> MaskParam;
        protected bool? RectMask2DParam;
        protected string FillColorParam;
        protected Dictionary<string, object> addComponentJson;
        protected List<object> componentsJson;

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
            _areaCache = CalcAreaInternal();
            _canvasGroupParam = json.GetDic("canvas_group");
            LayoutParam = json.GetDic("layout");
            ContentSizeFitterParam = json.GetDic("content_size_fitter");
            MaskParam = json.GetDic("mask");
            RectMask2DParam = json.GetBool("rect_mask_2d");
            FillColorParam = json.Get("fill_color");
            addComponentJson = json.GetDic("add_component");
            componentsJson = json.Get<List<object>>("components");
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
            ElementUtil.SetupCanvasGroup(go, _canvasGroupParam);
            ElementUtil.SetupChildImageComponent(go, RenderedChildren);
            ElementUtil.SetupFillColor(go, FillColorParam);
            ElementUtil.SetupContentSizeFitter(go, ContentSizeFitterParam);
            ElementUtil.SetupLayoutGroup(go, LayoutParam);
            ElementUtil.SetupLayoutElement(go, LayoutElementParam);
            ElementUtil.SetupComponents(go, componentsJson);
            ElementUtil.SetupMask(go, MaskParam);

            ElementUtil.SetRectTransform(go, rectTransformJson);

            return go;
        }


        protected virtual GameObject CreateSelf(RenderContext renderContext)
        {
            var go = CreateUIGameObject(renderContext);

            var rect = go.GetComponent<RectTransform>();
            var area = CalcArea();
            //rect.sizeDelta = area.Size;
            //rect.anchoredPosition = renderer.CalcPosition(area.Min, area.Size);

            //SetMaskImage(renderer, go);
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

        private Area CalcAreaInternal()
        {
            var area = Area.None();
            foreach (var element in Elements) area.Merge(element.CalcArea());
            return area;
        }

        public override Area CalcArea()
        {
            return _areaCache;
        }
    }
}