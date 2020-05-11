using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// ViewportElement class.
    /// </summary>
    public sealed class ViewportElement : GroupElement
    {
        protected readonly Dictionary<string, object> ScrollRectJson;
        protected readonly Dictionary<string, object> ContentJson;
        protected Element ParentElement;

        public ViewportElement(Dictionary<string, object> json, Element parent) : base(json, parent, true)
        {
            ScrollRectJson = json.GetDic("scroll_rect");
            ContentJson = json.GetDic("content");
            ParentElement = parent;
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateSelf(renderContext);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                // 親のパラメータがある場合､親にする
                // 後のAnchor設定のため これ以降でないと正確に設定できない
                rect.SetParent(parentObject.transform);
            }

            ElementUtil.SetLayer(go, Layer);
            ElementUtil.SetupRectTransform(go, RectTransformJson);

            // タッチイベントを取得するイメージコンポーネントになる
            ElementUtil.SetupFillColor(go, FillColorJson);

            // コンテンツ部分を入れるコンテナ
            var goContent = new GameObject("$Content");
            ElementUtil.SetLayer(goContent, Layer); // Viewportと同じレイヤー
            var contentRect = goContent.AddComponent<RectTransform>();
            goContent.transform.SetParent(go.transform);

            if (ContentJson != null)
            {
                goContent.name = ContentJson.Get("name") ?? "";

                if (ContentJson.ContainsKey("pivot"))
                    // ここのPivotはX,Yでくる
                    contentRect.pivot = ContentJson.GetDic("pivot").GetVector2("x", "y").Value;
                if (ContentJson.ContainsKey("anchor_min"))
                    contentRect.anchorMin = ContentJson.GetDic("anchor_min").GetVector2("x", "y").Value;
                if (ContentJson.ContainsKey("anchor_max"))
                    contentRect.anchorMax = ContentJson.GetDic("anchor_max").GetVector2("x", "y").Value;
                if (ContentJson.ContainsKey("offset_min"))
                    contentRect.offsetMin = ContentJson.GetDic("offset_min").GetVector2("x", "y").Value;
                if (ContentJson.ContainsKey("offset_max"))
                    contentRect.offsetMax = ContentJson.GetDic("offset_max").GetVector2("x", "y").Value;

                if (ContentJson.ContainsKey("layout"))
                {
                    var contentLayout = ContentJson.GetDic("layout");
                    ElementUtil.SetupLayoutGroup(goContent, contentLayout);
                }

                if (ContentJson.ContainsKey("content_size_fitter"))
                {
                    var contentSizeFitter = ContentJson.GetDic("content_size_fitter");
                    var compSizeFitter = ElementUtil.SetupContentSizeFitter(goContent, contentSizeFitter);
                }
            }

            //Viewportのチャイルドはもとより、content向けのAnchor・Offsetを持っている
            RenderChildren(renderContext, goContent);

            ElementUtil.SetupRectMask2D(go, RectMask2D);
            // ScrollRectを設定した時点ではみでたContentがアジャストされる　PivotがViewport内に入っていればOK
            ElementUtil.SetupScrollRect(go, goContent, ScrollRectJson);

            return go;
        }


        public override void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
            var self = selfAndSiblings.Find(tuple => tuple.Item2 == this);
            var scrollRect = self.Item1.GetComponent<ScrollRect>();
            var scrollbars = selfAndSiblings
                .Where(goElem => goElem.Item2 is ScrollbarElement) // 兄弟の中からScrollbarを探す
                .Select(goElem => goElem.Item1.GetComponent<Scrollbar>()) // ScrollbarコンポーネントをSelect
                .ToList();
            scrollbars.ForEach(scrollbar =>
            {
                switch (scrollbar.direction)
                {
                    case Scrollbar.Direction.LeftToRight:
                        scrollRect.horizontalScrollbar = scrollbar;
                        break;
                    case Scrollbar.Direction.RightToLeft:
                        scrollRect.horizontalScrollbar = scrollbar;
                        break;
                    case Scrollbar.Direction.BottomToTop:
                        scrollRect.verticalScrollbar = scrollbar;
                        break;
                    case Scrollbar.Direction.TopToBottom:
                        scrollRect.verticalScrollbar = scrollbar;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }
    }
}