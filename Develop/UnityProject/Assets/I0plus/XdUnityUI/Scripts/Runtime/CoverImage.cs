using System;
using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI
{
    [ExecuteAlways]
    public class CoverImage : MonoBehaviour
    {
        private float _parentWidth;
        private float _parentHeight;
        private float _preferredWidth;
        private float _preferredHeight;

        protected Image CachedImage
        {
            get
            {
                if (_cachedImage == null) _cachedImage = this.GetComponent<Image>();
                return _cachedImage;
            }
        }

        private Image _cachedImage;

        private void OnEnable()
        {
            var rect = this.transform as RectTransform;
            var center = new Vector2(0.5f, 0.5f);
            rect.anchorMin = center;
            rect.anchorMax = center;
        }

        private void Update()
        {
            var image = CachedImage;
            if (image == null) return;
            
            var parentTransform = this.transform.parent as RectTransform;
            var parentWidth = parentTransform.rect.width;
            var parentHeight = parentTransform.rect.height;
            var preferredWidth = image.preferredWidth;
            var preferredHeight = image.preferredHeight;

            if (parentWidth == _parentWidth && parentHeight == _parentHeight && preferredWidth == _preferredWidth &&
                preferredHeight == _preferredHeight)
            {
                return;
            }

            var narrow = (((float) parentHeight / parentWidth) <= (preferredHeight / preferredWidth));
            var rect = this.transform as RectTransform;
            rect.sizeDelta = narrow
                ? new Vector2(parentWidth, preferredHeight * parentWidth / preferredWidth)
                : new Vector2(preferredWidth * parentHeight / preferredHeight, parentHeight);

            _parentWidth = parentWidth;
            _parentHeight = parentHeight;
            _preferredWidth = preferredWidth;
            _preferredHeight = preferredHeight;
        }
    }
}