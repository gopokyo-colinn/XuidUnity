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
        private Vector2? canvasPosition;
        private Vector2? sizeDelta;

        private Dictionary<string, object> _scrollRect;
        private Dictionary<string, object> _content;
        private Element parentElement;

        public ViewportElement(Dictionary<string, object> json, Element parent) : base(json, parent, true)
        {
            canvasPosition = json.GetVector2("x", "y");
            sizeDelta = json.GetVector2("w", "h");
            _scrollRect = json.GetDic("scroll_rect");
            _content = json.GetDic("content");
            parentElement = parent;
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

            ElementUtil.SetLayer(go, layer);
            ElementUtil.SetRectTransform(go, rectTransformJson);

            // タッチイベントを取得するイメージコンポーネントになる
            ElementUtil.SetupFillColor(go, FillColorParam);

            // コンテンツ部分を入れるコンテナ
            var goContent = new GameObject("$Content");
            ElementUtil.SetLayer(goContent, layer); // Viewportと同じレイヤー
            var contentRect = goContent.AddComponent<RectTransform>();
            goContent.transform.SetParent(go.transform);

            if (_content != null)
            {
                goContent.name = _content.Get("name") ?? "";

                if (_content.ContainsKey("pivot"))
                    // ここのPivotはX,Yでくる
                    contentRect.pivot = _content.GetDic("pivot").GetVector2("x", "y").Value;
                if (_content.ContainsKey("anchor_min"))
                    contentRect.anchorMin = _content.GetDic("anchor_min").GetVector2("x", "y").Value;
                if (_content.ContainsKey("anchor_max"))
                    contentRect.anchorMax = _content.GetDic("anchor_max").GetVector2("x", "y").Value;
                if (_content.ContainsKey("offset_min"))
                    contentRect.offsetMin = _content.GetDic("offset_min").GetVector2("x", "y").Value;
                if (_content.ContainsKey("offset_max"))
                    contentRect.offsetMax = _content.GetDic("offset_max").GetVector2("x", "y").Value;

                if (_content.ContainsKey("layout"))
                {
                    var contentLayout = _content.GetDic("layout");
                    ElementUtil.SetupLayoutGroup(goContent, contentLayout);
                }

                if (_content.ContainsKey("content_size_fitter"))
                {
                    var contentSizeFitter = _content.GetDic("content_size_fitter");
                    var compSizeFitter = ElementUtil.SetupContentSizeFitter(goContent, contentSizeFitter);
                }
            }

            //Viewportのチャイルドはもとより、content向けのAnchor・Offsetを持っている
            RenderChildren(renderContext, goContent);

            ElementUtil.SetupRectMask2D(go, RectMask2DParam);
            // ScrollRectを設定した時点ではみでたContentがアジャストされる　PivotがViewport内に入っていればOK
            ElementUtil.SetupScrollRect(go, goContent, _scrollRect);

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


        public override Area CalcArea()
        {
            return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
        }
    }
}