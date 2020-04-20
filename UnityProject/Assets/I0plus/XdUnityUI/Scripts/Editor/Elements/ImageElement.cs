using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// ImageElement class.
    /// based on Baum2.Editor.ImageElement class.
    /// </summary>
    public class ImageElement : Element
    {
        //private string spriteName;

        //private Vector2? canvasPosition;
        //private Vector2? sizeDelta;
        public Dictionary<string, object> component;
        public Dictionary<string, object> imageJson;

        public ImageElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            //canvasPosition = json.GetVector2("x", "y");
            //sizeDelta = json.GetVector2("w", "h");
            component = json.GetDic("component");
            imageJson = json.GetDic("image");
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateUIGameObject(renderContext);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            var image = go.AddComponent<Image>();
            var sourceImage = imageJson.Get("source_image");
            if (sourceImage != null)
                image.sprite = renderContext.GetSprite(sourceImage);
            
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            var raycastTarget = imageJson.GetBool("raycast_target");
            if (raycastTarget != null)
                image.raycastTarget = raycastTarget.Value;

            image.type = Image.Type.Sliced;
            var imageType = imageJson.Get("image_type");
            if (imageType != null)
            {
                switch (imageType.ToLower())
                {
                    case "sliced":
                        image.type = Image.Type.Sliced;
                        break;
                    case "filled":
                        image.type = Image.Type.Filled;
                        break;
                    case "tiled":
                        image.type = Image.Type.Tiled;
                        break;
                    case "simple":
                        image.type = Image.Type.Simple;
                        break;
                    default:
                        Debug.LogAssertion("[XdUnityUI] unknown image_type:" + imageType);
                        break;
                }
            }

            var preserveAspect = imageJson.GetBool("preserve_aspect");
            if (preserveAspect != null && preserveAspect.Value )
            {
                // アスペクト比を保つ場合はSimpleにする
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            SetAnchor(go, renderContext);

            return go;
        }

        public override Area CalcArea()
        {
            /*
            if (canvasPosition != null && sizeDelta != null)
                return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
                */
            return null;
        }
    }
}