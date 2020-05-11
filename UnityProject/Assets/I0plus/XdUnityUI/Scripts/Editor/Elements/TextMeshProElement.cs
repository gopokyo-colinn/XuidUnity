using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

namespace XdUnityUI.Editor
{
    /// <summary>
    /// TextMeshProElement class.
    /// </summary>
    public sealed class TextMeshProElement : Element
    {
#if TMP_PRESENT
        protected readonly Dictionary<string, object> textJson;

        public TextMeshProElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            textJson = json.GetDic("text");
        }


        public override GameObject Render(RenderContext renderer, GameObject parentObject)
        {
            var go = CreateUiGameObject(renderer);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            
            var message = textJson.Get("text");
            var fontName = textJson.Get("font");
            var fontStyle = textJson.Get("style");
            var fontSize = textJson.GetFloat("size");
            var align = textJson.Get("align");
            var type = textJson.Get("textType");
            var virtualHeight = textJson.GetFloat("vh");

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.font = renderer.GetTMPFontAsset(fontName, fontStyle);
            text.fontSize = fontSize.Value;
            
            // 自動的に改行されることが困ることもあるが、挙動としてはこちらのほうがXDに沿うことになる
            text.textInfo.textComponent.enableWordWrapping = true;

            var color = textJson.Get("color");
            if (color != null)
            {
                text.color = EditorUtil.HexToColor(color);
            }
            
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
                {
                    middle = false;
                }
                else
                {
                    middle = !message.Contains("\n");
                }
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            // var fixedPos = rect.anchoredPosition;
            if (align.Contains("left"))
            {
                text.alignment = middle ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.TopLeft;
            }
            else if (align.Contains("center"))
            {
                text.alignment = middle ? TextAlignmentOptions.Midline : TextAlignmentOptions.Top;
            }
            else if (align.Contains("right"))
            {
                text.alignment = middle ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.TopRight;
            }

            if (textJson.ContainsKey("strokeSize"))
            {
                var strokeSize = textJson.GetInt("strokeSize");
                var strokeColor = EditorUtil.HexToColor(textJson.Get("strokeColor"));
                var outline = go.AddComponent<Outline>();
                outline.effectColor = strokeColor;
                outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            ElementUtil.SetupRectTransform(go, RectTransformJson);
            return go;
        }
#endif
    }
}