#if TMP_PRESENT
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

#endif

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     TextMeshProElement class.
    /// </summary>
#if TMP_PRESENT
    public sealed class TextMeshProElement : Element
    {
        private readonly Dictionary<string, object> _textJson;

        public TextMeshProElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            _textJson = json.GetDic("text");
        }


        public override GameObject Render(RenderContext renderer, GameObject parentObject)
        {
            bool isPrefabChild;
            var go = CreateUiGameObject(renderer, parentObject, out isPrefabChild);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject && !isPrefabChild)
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);

            var message = _textJson.Get("text");
            var fontName = _textJson.Get("font");
            var fontStyle = _textJson.Get("style");
            var fontSize = _textJson.GetFloat("size");
            var align = _textJson.Get("align");
            var type = _textJson.Get("textType");

            var text = go.GetComponent<TextMeshProUGUI>();

            //if a text component is already present this means this go is part of a prefab and we skip the font generation
            if (text == null)
            {
                text = go.AddComponent<TextMeshProUGUI>();
                text.font = renderer.GetTMPFontAsset(fontName, fontStyle);
            }

            text.text = message;
            text.fontSize = fontSize.Value;

            // 自動的に改行されることが困ることもあるが、挙動としてはこちらのほうがXDに沿うことになる
            if (text.textInfo != null && text.textInfo.textComponent != null)
                text.textInfo.textComponent.enableWordWrapping = true;
            else
                // Essential がインストールしてない場合に　NULLになる可能性
                Debug.LogError("[XdUnityUI] Can not access text.textInfo.textComponent");

            var color = _textJson.Get("color");
            if (color != null) text.color = EditorUtil.HexToColor(color);

            // BAUM2からTextMeshProへの変換を行うと少し横にひろがってしまうことへの対応
            // text.textInfo.textComponent.characterSpacing = -1.7f; // 文字幅を狭める

            var middle = true;
            if (type == "point")
            {
                text.horizontalMapping = TextureMappingOptions.Line;
                text.verticalMapping = TextureMappingOptions.Line;
                middle = true;
            }
            else if (type == "paragraph")
            {
                text.horizontalMapping = TextureMappingOptions.Paragraph;
                text.verticalMapping = TextureMappingOptions.Line;
                if (align.Contains("upper"))
                    middle = false;
                else
                    middle = !message.Contains("\n");
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            // var fixedPos = rect.anchoredPosition;
            if (align.Contains("left"))
                text.alignment = middle ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.TopLeft;
            else if (align.Contains("center"))
                text.alignment = middle ? TextAlignmentOptions.Midline : TextAlignmentOptions.Top;
            else if (align.Contains("right"))
                text.alignment = middle ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.TopRight;

            if (_textJson.ContainsKey("strokeSize"))
            {
                var strokeSize = _textJson.GetInt("strokeSize");
                var strokeColor = EditorUtil.HexToColor(_textJson.Get("strokeColor"));
                var outline = AddComponent<Outline>();
                outline.effectColor = strokeColor;
                outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            ElementUtil.SetupRectTransform(go, RectTransformJson);
            return go;
        }
    }
#endif
}