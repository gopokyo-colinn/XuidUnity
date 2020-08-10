using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I0plus.XdUnityUI.Editor
{
    public class InstanceElement : Element
    {
        private readonly string master;

        public InstanceElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            master = json.Get("master");
        }

        public override void Render(RenderContext renderContext, ref GameObject go, GameObject parentObject)
        {
            var path = EditorUtil.GetOutputPrefabsFolderAssetPath() + "/" + master + ".prefab";

            var prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabObject == null)
            {
                // 読み込むPrefabが存在しなかった
                // ダミーのPrefabを作成する
                var tempObject = new GameObject("temporary object");
                tempObject.AddComponent<RectTransform>();
                // ダミーとわかるようにmagentaイメージを置く -> non-destructiive importで、このイメージを採用してしまうためコメントアウト
                // var image = tempObject.AddComponent<Image>();
                // image.color = Color.magenta;
                // フォルダの用意
                Importer.CreateFolderRecursively(path.Substring(0, path.LastIndexOf('/')));
                // prefabの作成
                var savedAsset = PrefabUtility.SaveAsPrefabAsset(tempObject, path);
                AssetDatabase.Refresh();
                Debug.Log($"[XdUnityUI] Created temporary prefab. {path}", savedAsset);
                Object.DestroyImmediate(tempObject);
                prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            go = GetSelfObject(renderContext, parentObject);
            if (go == null) go = (GameObject) PrefabUtility.InstantiatePrefab(prefabObject);

            var rect = GetOrAddComponent<RectTransform>(go);
            rect.SetParent(parentObject.transform);

            go.name = Name;
            ElementUtil.SetLayer(go, Layer);
            ElementUtil.SetupRectTransform(go, RectTransformJson);
            if (Active != null) go.SetActive(Active.Value);
            ElementUtil.SetupLayoutElement(go, LayoutElementJson);
        }
    }
}