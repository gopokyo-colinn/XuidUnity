using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     ButtonElement class.
    ///     based on Baum2.Editor.ButtonElement class.
    /// </summary>
    public class ButtonElement : GroupElement
    {
        protected readonly Dictionary<string, object> ButtonJson;

        public ButtonElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            ButtonJson = json.GetDic("button");
        }

        public override GameObject Render(RenderContext renderContext, GameObject parentObject)
        {
            var go = CreateSelf(renderContext, parentObject);

            var children = RenderChildren(renderContext, go);
            var deleteObjects = new Dictionary<GameObject, bool>();

            var button = AddComponent<Button>();


            GameObject targetImageObject = null;
            if (ButtonJson != null)
            {
                var targetImage =
                    ElementUtil.FindComponentByClassName<Image>(children, ButtonJson.Get("target_graphic"));
                if (targetImage != null)
                {
                    button.targetGraphic = targetImage;
                    targetImageObject = targetImage.gameObject;
                }

                // すげ替え画像を探し、設定する
                // 見つかった場合は
                // その画像オブジェクトを削除し
                // SpriteSwapに設定する
                var spriteStateJson = ButtonJson.GetDic("sprite_state");
                if (spriteStateJson != null)
                {
                    var spriteState = new SpriteState();
                    var image = ElementUtil.FindComponentByClassName<Image>(children,
                        spriteStateJson.Get("highlighted_sprite_target"));
                    if (image != null)
                    {
                        spriteState.highlightedSprite = image.sprite;
                        deleteObjects[image.gameObject] = true;
                        button.transition = Selectable.Transition.SpriteSwap;
                    }

                    image = ElementUtil.FindComponentByClassName<Image>(children,
                        spriteStateJson.Get("pressed_sprite_target"));
                    if (image != null)
                    {
                        spriteState.pressedSprite = image.sprite;
                        deleteObjects[image.gameObject] = true;
                        button.transition = Selectable.Transition.SpriteSwap;
                    }

#if UNITY_2019_1_OR_NEWER
                    image = ElementUtil.FindComponentByClassName<Image>(children,
                        spriteStateJson.Get("selected_sprite_target"));
                    if (image != null)
                    {
                        spriteState.selectedSprite = image.sprite;
                        deleteObjects[image.gameObject] = true;
                    }
#endif

                    image = ElementUtil.FindComponentByClassName<Image>(children,
                        spriteStateJson.Get("disabled_sprite_target"));
                    if (image != null)
                    {
                        spriteState.disabledSprite = image.sprite;
                        deleteObjects[image.gameObject] = true;
                        button.transition = Selectable.Transition.SpriteSwap;
                    }

                    button.spriteState = spriteState;
                }

                // transitionの設定が明記してある場合は上書き設定する
                var type = ButtonJson.Get("transition");
                switch (type)
                {
                    case "sprite-swap":
                        button.transition = Selectable.Transition.SpriteSwap;
                        break;
                    case "color-tint":
                        button.transition = Selectable.Transition.ColorTint;
                        break;
                    case "animation":
                        button.transition = Selectable.Transition.Animation;
                        break;
                    case "none":
                        button.transition = Selectable.Transition.None;
                        break;
                }
            }

            foreach (var keyValuePair in deleteObjects)
                if (keyValuePair.Key != targetImageObject)
                    Object.DestroyImmediate(keyValuePair.Key);

            // TargetGraphicが設定されなかった場合
            if (button.targetGraphic == null)
            {
                // 子供からImage持ちを探す

                var image = go.GetComponentInChildren<Image>();
                if (image == null)
                    // componentでないか探す
                    image = go.GetComponent<Image>();

                button.targetGraphic = image;
            }

            ElementUtil.SetupRectTransform(go, RectTransformJson);
            ElementUtil.SetupLayoutElement(go, LayoutElementJson);
            ElementUtil.SetupComponents(go, ComponentsJson);
            return go;
        }
    }
}