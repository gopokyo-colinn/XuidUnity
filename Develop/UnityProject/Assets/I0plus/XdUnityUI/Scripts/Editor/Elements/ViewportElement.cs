using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// ViewportElement class.
    /// GroupElementに統合した
    /// 削除予定
    /// </summary>
    public sealed class ViewportElement : GroupElement
    {
        private readonly Dictionary<string, object> _scrollRectJson = default;
        private readonly Dictionary<string, object> _contentJson = default;
        private Element _parentElement = default;

        public ViewportElement(Dictionary<string, object> json, Element parent) : base(json, parent, true)
        {
            _scrollRectJson = json.GetDic("scroll_rect");
            _contentJson = json.GetDic("content");
            _parentElement = parent;
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

            if (_contentJson != null)
            {
                goContent.name = _contentJson.Get("name") ?? "";
                var rectJson = _contentJson.GetDic("rect_transform");
                if (rectJson != null)
                {
                    ElementUtil.SetupRectTransform(goContent,rectJson);
                }

                if (_contentJson.ContainsKey("layout"))
                {
                    var contentLayout = _contentJson.GetDic("layout");
                    ElementUtil.SetupLayoutGroup(goContent, contentLayout);
                }

                if (_contentJson.ContainsKey("content_size_fitter"))
                {
                    var contentSizeFitter = _contentJson.GetDic("content_size_fitter");
                    var compSizeFitter = ElementUtil.SetupContentSizeFitter(goContent, contentSizeFitter);
                }
            }

            //Viewportのチャイルドはもとより、content向けのAnchor・Offsetを持っている
            RenderChildren(renderContext, goContent);

            ElementUtil.SetupRectMask2D(go, RectMask2D);
            // ScrollRectを設定した時点ではみでたContentがアジャストされる　PivotがViewport内に入っていればOK
            ElementUtil.SetupScrollRect(go, goContent, _scrollRectJson);

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