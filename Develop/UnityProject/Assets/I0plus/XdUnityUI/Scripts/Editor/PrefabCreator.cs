using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniJSON;
using UnityEditor;
using UnityEngine;

#if TMP_PRESENT
using TMPro;
#endif

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     PrefabCreator class.
    ///     based on Baum2.Editor.PrefabCreator class.
    /// </summary>
    public sealed class PrefabCreator
    {
        private static readonly string[] Versions = {"0.6.0", "0.6.1"};
        private readonly string assetPath;
        private readonly string fontRootPath;
        private readonly string spriteRootPath;
        private readonly List<GameObject> nestedPrefabs;
        /// <summary>
        /// </summary>
        /// <param name="spriteRootPath"></param>
        /// <param name="fontRootPath"></param>
        /// <param name="assetPath">フルパスでの指定 Unity Assetフォルダ外もよみこめる</param>
        public PrefabCreator(string spriteRootPath, string fontRootPath, string assetPath, List<GameObject> prefabs)
        {
            this.spriteRootPath = spriteRootPath;
            this.fontRootPath = fontRootPath;
            this.assetPath = assetPath;
            this.nestedPrefabs = prefabs;
        }

        public GameObject Create()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

            var text = File.ReadAllText(assetPath);
            var json = Json.Deserialize(text) as Dictionary<string, object>;
            var info = json.GetDic("info");
            Validation(info);

            var renderer = new RenderContext(spriteRootPath, fontRootPath, nestedPrefabs);
            var rootJson = json.GetDic("root");
            GameObject root = null;

            var rootElement = ElementFactory.Generate(rootJson, null);
            root = rootElement.Render(renderer, null);

            Postprocess(root);

            if (renderer.ToggleGroupMap.Count > 0)
            {
                // ToggleGroupが作成された場合
                var go = new GameObject("ToggleGroup");
                go.transform.SetParent(root.transform);
                foreach (var keyValuePair in renderer.ToggleGroupMap)
                {
                    var gameObject = keyValuePair.Value;
                    gameObject.transform.SetParent(go.transform);
                }
            }

            foreach(var prefab in renderer.NewPrefabs.ToList())
            {

                //if we haven't created a prefab out of the referenced GO we do so now
                if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
                {
                    //TODO: Ugly path generation
                    var nestedPrefabDirectory = Path.Combine(Application.dataPath.Replace("Assets", ""), Path.Combine(Path.Combine(EditorUtil.GetOutputPrefabsFolderAssetPath()), "Components"));

                    if (!Directory.Exists(nestedPrefabDirectory))
                        Directory.CreateDirectory(nestedPrefabDirectory);

                    nestedPrefabs.Add(UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, Path.Combine(nestedPrefabDirectory, prefab.name + ".prefab"), UnityEditor.InteractionMode.AutomatedAction));
                }
            }

            return root;
        }

        private void Postprocess(GameObject go)
        {
            var methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(BaumPostprocessor)))
                .Select(x => x.GetMethod("OnPostprocessPrefab"));
            foreach (var method in methods) method.Invoke(null, new object[] {go});
        }

        public void Validation(Dictionary<string, object> info)
        {
            var version = info.Get("version");
            if (!Versions.Contains(version))
                throw new Exception(string.Format("version {0} is not supported", version));
        }
    }
}