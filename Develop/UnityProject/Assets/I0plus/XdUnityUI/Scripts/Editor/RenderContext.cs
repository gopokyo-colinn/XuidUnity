using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace I0plus.XdUnityUI.Editor
{
    public class Identifier
    {
        public string Name { get; }
        public string XdGuid { get; }

        public Identifier(string name, string xdGuid)
        {
            Name = name;
            XdGuid = xdGuid;
        }
    }

    public class RenderContext
    {
        private readonly string spriteRootPath;
        private readonly string fontRootPath;
        private readonly GameObject rootObject;
        public Stack<GameObject> NewPrefabs { get; } = new Stack<GameObject>();
        public Dictionary<string, GameObject> ToggleGroupMap { get; } = new Dictionary<string, GameObject>();

        public Dictionary<GameObject, Identifier> FreeChildObjects { get; }

        /// <summary>
        /// XdGuid compornentをAddするかどうか
        /// </summary>
        public bool OptionAddXdGuid
        {
            get;
            set;
        }

        public ToggleGroup GetToggleGroup(string name)
        {
            ToggleGroup toggleGroup;
            if (!ToggleGroupMap.ContainsKey(name))
            {
                // まだそのグループが存在しない場合は､GameObjectを作成
                var go = new GameObject(name);
                // AddComponent･登録する
                toggleGroup = go.AddComponent<ToggleGroup>();
                // Allow Switch Off を True にする
                // 190711 false(デフォルト)だと DoozyUIがHideするときに､トグルONボタンを初期位置に戻してしまうため
                toggleGroup.allowSwitchOff = true;
                ToggleGroupMap[name] = go;
            }
            else
            {
                // 存在する場合は利用する
                toggleGroup = ToggleGroupMap[name].GetComponent<ToggleGroup>();
            }

            return toggleGroup;
        }

        public RenderContext(string spriteRootPath, string fontRootPath, GameObject rootObject)
        {
            this.spriteRootPath = spriteRootPath;
            this.fontRootPath = fontRootPath;
            this.rootObject = rootObject;
            OptionAddXdGuid = false;
            FreeChildObjects = new Dictionary<GameObject, Identifier>();
            if (rootObject != null)
            {
                var rects = rootObject.GetComponentsInChildren<RectTransform>();
                foreach (var rect in rects)
                {
                    // 後の名前検索で正確にできるように/を前にいれる
                    var name = "/" + rect.gameObject.name;
                    var parent = rect.parent;
                    while (parent)
                    {
                        name = "/" + parent.name + name;
                        parent = parent.parent;
                    }

                    string xdGuid = null;
                    var xdGuidComponent = rect.gameObject.GetComponent<XdGuid>();
                    if (xdGuidComponent != null)
                    {
                        xdGuid = xdGuidComponent.guid;
                    }

                    FreeChildObjects.Add(rect.gameObject, new Identifier(name: name, xdGuid: xdGuid));
                }
            }
        }

        private Transform RecursiveFindChild(Transform parent, string childName)
        {
            var foundChild = parent.Find(childName);
            if (foundChild) return foundChild;
            foreach (Transform child in parent)
            {
                var found = RecursiveFindChild(child, childName);
                if (found != null) return found;
            }

            return null;
        }

        public GameObject FindObject(string name)
        {
            if (rootObject == null || rootObject.transform == null) return null;
            var findTransform = RecursiveFindChild(rootObject.transform, name);
            if (findTransform == null) return null;
            return findTransform.gameObject;
        }

        /// <summary>
        ///     親の名前も使用して検索する
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public GameObject FindObject(string name, GameObject parentObject)
        {
            // 出来るだけユニークな名前になるように、Rootからの名前を作成する
            var findNames = new List<string> {name};
            var fullName = name;
            while (parentObject != null)
            {
                fullName = parentObject.name + "/" + fullName;
                findNames.Add(fullName);
                var parent = parentObject.transform.parent;
                parentObject = parent ? parent.gameObject : null;
            }

            findNames.Reverse();

            // Rootから親のパス付名 → 単体の名前の順に検索する
            foreach (var findName in findNames)
            {
                var selfObject = FindObject(findName);
                if (selfObject != null)
                    // Debug.Log($"GetSelfObject({findName})");
                    return selfObject;
            }

            return null;
        }

        public List<GameObject> FindObjects(string name, GameObject parentObject)
        {
            // 出来るだけユニークな名前になるように、Rootからの名前を作成する
            var findNames = new List<string> {name};
            var fullName = "/" + name;
            while (parentObject != null)
            {
                fullName = "/" + parentObject.name + fullName;
                findNames.Add(fullName);
                var parent = parentObject.transform.parent;
                parentObject = parent ? parent.gameObject : null;
            }

            // Rootから親のパス付名 → 単体の名前の順に検索する
            findNames.Reverse();

            var founds = new List<GameObject>();
            foreach (var findName in findNames)
            {
                foreach (var keyValuePair in FreeChildObjects)
                {
                    var pathName = keyValuePair.Value.Name;
                    var xdGuid = keyValuePair.Value.XdGuid;
                    if (pathName != null && pathName.EndsWith(findName))
                        founds.Add(keyValuePair.Key);
                }

                if (founds.Count > 0) break;
            }

            return founds;
        }

        public GameObject FindObjectFromXdGuid(string guid)
        {
            foreach (var freeChildObject in FreeChildObjects)
            {
                var xdGuid = freeChildObject.Value.XdGuid;
                if (xdGuid == guid) return freeChildObject.Key;
            }

            return null;
        }

        public GameObject OccupyObject(string guid, string name, GameObject parentObject)
        {
            GameObject found = null;
            if (guid != null)
            {
                found = FindObjectFromXdGuid(guid);
            }

            if (found == null && name != null)
            {
                // Debug.Log($"guidで見つからなかった:{name}");
                var founds = FindObjects(name, parentObject);
                if (founds == null || founds.Count == 0) return null;
                found = founds[0];
            }

            if (found != null)
            {
                FreeChildObjects.Remove(found);
            }
            
            return found;
        }

        public Sprite GetSprite(string spriteName)
        {
            var path = Path.Combine(spriteRootPath, spriteName) + ".png";
            // 相対パスの記述に対応した
            var fileInfo = new FileInfo(path);
            var fullName = TextureUtil.GetSameImagePath(fileInfo.FullName);
            // TextureUtil.SliceSprite(fullName);
            var unityPath = EditorUtil.ToAssetPath(fullName);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(unityPath);
            if (sprite == null) Debug.LogError($"[XdUnityUI] sprite \"{unityPath}\" is not found.");

            return sprite;
        }

        public Font GetFont(string fontName)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".ttf");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".otf");
            if (font == null) font = Resources.GetBuiltinResource<Font>(fontName + ".ttf");
            if (font == null) Debug.LogError($"[XdUnityUI] font {fontName}.ttf (or .otf) is not found");

            return font;
        }

#if TMP_PRESENT
        public TMP_FontAsset GetTMPFontAsset(string fontName, string style)
        {
            var fontFileName = Path.Combine(fontRootPath, fontName) + "-" + style + " SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontFileName);
            if (font == null)
            {
                Debug.LogError(string.Format($"[XdUnityUI] TMP_FontAsset {fontFileName} is not found"));
            }

            return font;
        }
#endif
    }
}