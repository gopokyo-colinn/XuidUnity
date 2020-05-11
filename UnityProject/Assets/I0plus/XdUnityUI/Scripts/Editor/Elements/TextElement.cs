using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Baum2;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// TextElement class.
    /// based on Baum2.Editor.TextElement class.
    /// </summary>
    public sealed class TextElement : Element
    {
        protected readonly Dictionary<string, object> textJson;

        public TextElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            textJson = json.GetDic("text");
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateUiGameObject(renderContext);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            
            var message = textJson.Get("text");
            var fontName = textJson.Get("font");
            var fontSize = textJson.GetFloat("size");
            var align = textJson.Get("align");
            var type = textJson.Get("textType");

            var virtualHeight = textJson.GetFloat("vh");

            var raw = go.AddComponent<RawData>();
            raw.Info["font_size"] = fontSize;
            raw.Info["align"] = align;

            var text = go.AddComponent<Text>();
            text.text = message;

            // 検索するフォント名を決定する
            var fontFilename = fontName;
            
            if (textJson.ContainsKey("style"))
            {
                var style = textJson.Get("style");
                fontFilename += "-" + style;
                if (style.Contains("normal") || style.Contains("medium"))
                {
                    text.fontStyle = FontStyle.Normal;
                }

                if (style.Contains("bold"))
                {
                    text.fontStyle = FontStyle.Bold;
                }
            }

            text.font = renderContext.GetFont(fontFilename);
            text.fontSize = Mathf.RoundToInt(fontSize.Value);
            text.color = Color.black;

            var color = textJson.Get("color");
            text.color = color != null ? EditorUtil.HexToColor(color) : Color.black; 

            text.verticalOverflow = VerticalWrapMode.Truncate;

            if (type == "point")
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else if (type == "paragraph")
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            var vertical = "";
            var horizontal = "";
            var alignLowerString = align.ToLower();
            if (alignLowerString.Contains("left"))
            {
                horizontal = "left";
            }
            else if (alignLowerString.Contains("center"))
            {
                horizontal = "center";
            }
            else if (alignLowerString.Contains("right"))
            {
                horizontal = "right";
            }

            if (alignLowerString.Contains("upper"))
            {
                vertical = "upper";
            }
            else if (alignLowerString.Contains("middle"))
            {
                vertical = "middle";
            }
            else if (alignLowerString.Contains("lower"))
            {
                vertical = "lower";
            }

            switch ((vertical + "-" + horizontal).ToLower())
            {
                case "upper-left":
                    text.alignment = TextAnchor.UpperLeft;
                    break;
                case "upper-center":
                    text.alignment = TextAnchor.UpperCenter;
                    break;
                case "upper-right":
                    text.alignment = TextAnchor.UpperRight;
                    break;
                case "middle-left":
                    text.alignment = TextAnchor.MiddleLeft;
                    break;
                case "middle-center":
                    text.alignment = TextAnchor.MiddleCenter;
                    break;
                case "middle-right":
                    text.alignment = TextAnchor.MiddleRight;
                    break;
                case "lower-left":
                    text.alignment = TextAnchor.LowerLeft;
                    break;
                case "lower-center":
                    text.alignment = TextAnchor.LowerCenter;
                    break;
                case "lower-right":
                    text.alignment = TextAnchor.LowerRight;
                    break;
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
    }
}