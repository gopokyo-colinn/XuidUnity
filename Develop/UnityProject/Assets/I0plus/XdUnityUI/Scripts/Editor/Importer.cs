using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.U2D;
using UnityEngine.U2D;

#endif

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     based on Baum2/Editor/Scripts/BaumImporter file.
    /// </summary>
    public sealed class Importer : AssetPostprocessor
    {
        private static int _progressTotal = 1;
        private static int _progressCount;
        private static bool _autoEnableFlag; // デフォルトがチェック済みの時には true にする

        /// <summary>
        ///     自動インポート 自動削除
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        public static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var importFolderAssetPath = EditorUtil.GetImportFolderAssetPath();

            var forImportAssetPaths = new List<string>();
            foreach (var importedAsset in importedAssets)
            {
                if (importedAsset.StartsWith(importFolderAssetPath))
                {
                    forImportAssetPaths.Add(importedAsset);
                }
            }

            if (forImportAssetPaths.Count > 0)
            {
                await Import(forImportAssetPaths, false);

                // インポートしたファイルを削除し、そのフォルダが空になったらフォルダも削除する
                foreach (var forImportAssetPath in forImportAssetPaths)
                {
                    // フォルダの場合はスルー
                    if (IsFolder(forImportAssetPath))
                    {
                        continue;
                    }

                    // インポートするファイルを削除
                    AssetDatabase.DeleteAsset(forImportAssetPath);
                    // ファイルがあったフォルダが空になったかチェック
                    var folderName = Path.GetDirectoryName(forImportAssetPath);
                    var files = Directory.GetFiles(folderName);
                    if (files.Length == 0)
                    {
                        // フォルダの削除
                        AssetDatabase.DeleteAsset(folderName);
                    }
                }

                AssetDatabase.Refresh();
            }
        }

        public override int GetPostprocessOrder()
        {
            return 1000;
        }

        private static void UpdateDisplayProgressBar(string message = "")
        {
            if (_progressTotal > 1)
                EditorUtility.DisplayProgressBar("XdUnitUI Import",
                    $"{_progressCount}/{_progressTotal} {message}",
                    (float) _progressCount / _progressTotal);
        }

        /*
        [MenuItem("Assets/XdUnityUI/Import Selected Folders")]
        public static async Task MenuImportFromSelectFolder()
        {
            var folderPaths = ProjectHighlightedFolders();
            await ImportFolders(folderPaths, true, false);
        }
        */

        /*
        [MenuItem("Assets/XdUnityUI/Import Selected Folders", true)]
        public static bool MenuImportSelectedFolderCheck()
        {
            var folderPaths = ProjectHighlightedFolders();
            return folderPaths.Any();
        }
        */

        /*
        [MenuItem("Assets/XdUnityUI/Import Selected Folders(Layout Only)")]
        public static async Task MenuImportSelectedFolderLayoutOnly()
        {
            var folderPaths = ProjectHighlightedFolders();
            await ImportFolders(folderPaths, false, false);
        }
        */

        /*
        [MenuItem("Assets/XdUnityUI/Import Selected Folders(Layout Only)", true)]
        public static bool MenuImportSelectedFolderLayoutOnlyCheck()
        {
            var folderPaths = ProjectHighlightedFolders();
            return folderPaths.Any();
        }
        */

        [MenuItem("Assets/XdUnityUI/Clean Import...")]
        public static async Task MenuImportSpecifiedFolder()
        {
            var path = EditorUtility.OpenFolderPanel("Clean import:Specify Folder", "", "");
            if (string.IsNullOrWhiteSpace(path)) return;

            var folders = new List<string> {path};
            await ImportFolders(folders, false, true, false);
        }

        [MenuItem("Assets/XdUnityUI/(experimental)Overwrite Import...")]
        public static async Task MenuOverwriteImportSpecifiedFolder()
        {
            var path = EditorUtility.OpenFolderPanel("Overwrite Import:Specify Folder", "", "");
            if (string.IsNullOrWhiteSpace(path)) return;

            var folders = new List<string> {path};
            await ImportFolders(folders, true, true, false);
        }


        /*
        [MenuItem("Assets/XdUnityUI/Specify Folder Import(layout only)...")]
        public static async Task MenuImportSpecifiedFolderLayoutOnly()
        {
            var path = EditorUtility.OpenFolderPanel("Specify Exported Folder", "", "");
            if (string.IsNullOrWhiteSpace(path)) return;

            var folders = new List<string> {path};
            await ImportFolders(folders, false, false);
        }
        */

        /// <summary>
        ///     Project ウィンドウで、ハイライトされているディレクトリを取得する
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> ProjectHighlightedFolders()
        {
            var folders = new List<string>();

            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) folders.Add(path);
            }

            return folders;
        }


        private static async Task ImportFolders(IEnumerable<string> importFolderPaths, bool overwriteImportFlag,
            bool convertImageFlag,
            bool deleteAssetsFlag)
        {
            var importedAssets = new List<string>();

            foreach (var importFolderPath in importFolderPaths)
            {
                // トップディレクトリの追加
                // importedAssets.Add(importFolderPath);

                // var folders = Directory.EnumerateDirectories(importFolderPath);
                // importedAssets.AddRange(folders);

                // ファイルのリストアップ
                var files = Directory.EnumerateFiles(
                    importFolderPath, "*", SearchOption.AllDirectories);

                // 関係あるファイルのみ追加
                foreach (var file in files)
                {
                    if (!convertImageFlag && !file.EndsWith(".layout.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var extension = Path.GetExtension(file).ToLower();
                    if (extension == ".meta") continue;
                    importedAssets.Add(file);
                }
            }

            if (importedAssets.Count > 100)
            {
                var result = EditorUtility.DisplayDialog("Import",
                    $"Importing {importedAssets.Count} files.\n Continue?", "Continue", "Cancel");
                if (!result) return;
            }

            await Import(importedAssets, overwriteImportFlag);

            // インポートしたアセットのソース削除が必要ならここでするべきかも
            EditorUtility.DisplayDialog("Import", "Done.", "Ok");
        }

        private static bool IsFolder(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception exception)
            {
                // ignored
                Debug.LogError(exception.Message);
            }

            return false;
        }

        private class FolderInfos : Dictionary<string, HashSet<string>>
        {
            public void Add(string path, string fileExtension)
            {
                if (!this.Keys.Contains(path))
                {
                    var extentions = new HashSet<string>() {fileExtension};
                    this[path] = extentions;
                }

                this[path].Add(fileExtension);
            }
        };

        /// <summary>
        ///     Assetディレクトリに追加されたファイルを確認、インポート処理を行う
        /// </summary>
        /// <param name="importedAssetPaths"></param>
        /// <param name="overwriteImportFlag"></param>
        private static async Task Import(IEnumerable<string> importedAssetPaths, bool overwriteImportFlag)
        {
            var importedPaths = importedAssetPaths.ToList();
            _progressTotal = importedPaths.Count();
            if (_progressTotal == 0) return;
            _progressCount = 0;

            var changed = false;

            // インポートされたファイルからフォルダパスリストを作成する
            // Key: AssetPath
            // Value: ディレクトリにあるファイルの拡張子
            var importedFolderAssetPaths = new FolderInfos();
            foreach (var importedAssetPath in importedAssetPaths)
            {
                if (IsFolder(importedAssetPath))
                {
                    // すでにフォルダパスはスルー
                    continue;
                }

                var folderPath = Path.GetDirectoryName(importedAssetPath);
                var extension = Path.GetExtension(importedAssetPath);
                importedFolderAssetPaths.Add(folderPath, extension);
            }

            // 出力フォルダの作成
            foreach (var importedFolderInfo in importedFolderAssetPaths)
            {
                if (!IsFolder(importedFolderInfo.Key)) continue;

                // フォルダであった場合
                var importedFullPath = Path.GetFullPath(importedFolderInfo.Key);
                var subFolderName = Path.GetFileName(importedFolderInfo.Key);


                var isSpriteFolder = importedFolderInfo.Value.Contains(".png");
                // スプライト出力フォルダの準備
                if (isSpriteFolder)
                {
                    var outputSpritesFolderAssetPath = Path.Combine(
                        EditorUtil.GetOutputSpritesFolderAssetPath(), subFolderName);
                    if (Directory.Exists(outputSpritesFolderAssetPath))
                    {
                        // フォルダがすでにある　インポートファイルと比較して、出力先にある必要のないファイルを削除する
                        // ダブっている分は比較し、異なっている場合に上書きするようにする
                        var outputFolderInfo = new DirectoryInfo(outputSpritesFolderAssetPath);
                        var importFolderInfo = new DirectoryInfo(importedFullPath);

                        var existSpritePaths = outputFolderInfo.GetFiles("*.png", SearchOption.AllDirectories);
                        var importSpritePaths = importFolderInfo.GetFiles("*.png", SearchOption.AllDirectories);

                        // outputフォルダにある importにはないファイルをリストアップする
                        var deleteEntries = existSpritePaths.Except(importSpritePaths, new FileInfoComparer()).ToList();
                        // スプライト出力フォルダがすでにある場合はTextureハッシュキャッシュを削除する
                        deleteEntries.Add(new FileInfo(outputSpritesFolderAssetPath + "/" +
                                                       TextureUtil.ImageHashMapCacheFileName));
                        deleteEntries.Add(new FileInfo(outputSpritesFolderAssetPath + "/" +
                                                       TextureUtil.ImagePathMapCacheFileName));
                        // 削除する
                        foreach (var fileInfo in deleteEntries)
                        {
                            if (File.Exists(fileInfo.FullName))
                            {
                                File.Delete(fileInfo.FullName);
                            }

                            var metaFileName = fileInfo.FullName + ".meta";
                            if (File.Exists(metaFileName))
                            {
                                File.Delete(metaFileName);
                            }

                            changed = true;
                        }
                    }
                    else
                    {
                        AssetDatabase.CreateFolder(EditorUtil.GetOutputSpritesFolderAssetPath(),
                            subFolderName);
                        changed = true;
                    }
                }

                var prefabsOutputPath = Path.Combine(EditorUtil.GetOutputPrefabsFolderAssetPath(), subFolderName);
                if (!Directory.Exists(prefabsOutputPath))
                {
                    AssetDatabase.CreateFolder(EditorUtil.GetOutputPrefabsFolderAssetPath(),
                        subFolderName);
                    changed = true;
                }
            }

            if (changed)
            {
                // ディレクトリが作成されたり、画像が削除されるためRefresh
                AssetDatabase.Refresh();
                changed = false;
            }

            // フォルダが作成され、そこに画像を出力する場合
            // Refresh後、DelayCallで画像生成することで、処理が安定した
            await Task.Delay(1000);

            // SpriteイメージのハッシュMapをクリアしたかどうかのフラグ
            // importedAssetsに一気に全部の新規ファイルが入ってくる前提の処理
            // 全スライス処理が走る前、最初にClearImageMapをする
            var clearedImageMap = false;
            // 画像コンバート　スライス処理
            var messageCounter = new Dictionary<string, int>();
            var total = 0;
            foreach (var importedAsset in importedPaths)
            {
                if (!importedAsset.EndsWith(".png", StringComparison.Ordinal)) continue;
                //
                if (!clearedImageMap)
                {
                    clearedImageMap = true;
                }

                // スライス処理
                var message = TextureUtil.SliceSprite(importedAsset);
                changed = true;

                total++;
                _progressCount += 1;
                UpdateDisplayProgressBar(message);

                // 出力されたログをカウントする
                if (messageCounter.ContainsKey(message))
                    messageCounter[message] = messageCounter[message] + 1;
                else
                    messageCounter.Add(message, 1);
            }

            foreach (var keyValuePair in messageCounter)
                Debug.Log($"[XdUnityUI] {keyValuePair.Key} {keyValuePair.Value}/{total}");


            var importLayoutFilePaths = new List<string>();
            foreach (var layoutFilePath in importedPaths)
            {
                if (!layoutFilePath.EndsWith(".layout.json", StringComparison.OrdinalIgnoreCase)) continue;
                importLayoutFilePaths.Add(layoutFilePath);
            }

            string GetPrefabPath(string layoutFilePath)
            {
                var prefabFileName = Path.GetFileName(layoutFilePath).Replace(".layout.json", "") + ".prefab";
                var subFolderName = EditorUtil.GetSubFolderName(layoutFilePath);
                var saveAssetPath =
                    Path.Combine(Path.Combine(EditorUtil.GetOutputPrefabsFolderAssetPath(),
                        subFolderName), prefabFileName);
                return saveAssetPath;
            }

            /*
            foreach (var layoutFilePath in importedLayoutFilePaths)
            {
                var saveAssetPath = GetPrefabPath(layoutFilePath);
                var exists = AssetDatabase.LoadAssetAtPath(saveAssetPath, typeof(GameObject));
                if (exists == null)
                {
                    var go = new GameObject("not implement");
                    go.AddComponent<RectTransform>();
                    var savedAsset = PrefabUtility.SaveAsPrefabAsset(go, saveAssetPath);
                    Object.DestroyImmediate(go);
                }
            }
            */

            if (changed)
            {
                AssetDatabase.Refresh();
                changed = false;
            }

            await Task.Delay(1000);

            var prefabs = new List<GameObject>();

            // Create Prefab
            foreach (var layoutFilePath in importLayoutFilePaths)
            {
                UpdateDisplayProgressBar("layout");
                _progressCount += 1;
                GameObject go = null;
                try
                {
                    Debug.Log($"[XdUnityUI] in process...{Path.GetFileName(layoutFilePath)}");
                    // 
                    var subFolderName = EditorUtil.GetSubFolderName(layoutFilePath);
                    var saveAssetPath = GetPrefabPath(layoutFilePath);
                    var spriteOutputFolderAssetPath =
                        Path.Combine(EditorUtil.GetOutputSpritesFolderAssetPath(), subFolderName);
                    var fontAssetPath = EditorUtil.GetFontsAssetPath();

                    // overwriteImportFlagがTrueなら、ベースとなるPrefab上に生成していく
                    // 利用できるオブジェクトは利用していく
                    if (overwriteImportFlag)
                    {
                        // すでにあるプレハブを読み込む
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(saveAssetPath);
                        if (prefab != null)
                        {
                            go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot,
                                InteractionMode.AutomatedAction);
                        }
                    }

                    // Render Context
                    var renderContext = new RenderContext(spriteOutputFolderAssetPath, fontAssetPath, go);
                    if (overwriteImportFlag)
                    {
                        renderContext.OptionAddXdGuid = true;
                    }

                    // Create Prefab
                    var prefabCreator = new PrefabCreator(layoutFilePath, prefabs);
                    prefabCreator.Create(ref go, renderContext);
                    var savedAsset = PrefabUtility.SaveAsPrefabAsset(go, saveAssetPath);
                    Debug.Log($"[XdUnityUI] Created. {Path.GetFileName(saveAssetPath)}", savedAsset);
                }
                catch (Exception ex)
                {
                    Debug.LogAssertion("[XdUnityUI] " + ex.Message + "\n" + ex.StackTrace);
                    // 変換中例外が起きた場合もテンポラリGameObjectを削除する
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Import Failed", ex.Message, "Close");
                    throw;
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static void CreateSpritesFolder(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetFileName(asset));
            var directoryPath = EditorUtil.GetOutputSpritesFolderAssetPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            if (Directory.Exists(directoryFullPath))
                // 画像出力用フォルダに画像がのこっていればすべて削除
                // Debug.LogFormat("[XdUnityUI] Delete Exist Sprites: {0}", EditorUtil.ToUnityPath(directoryFullPath));
                foreach (var filePath in Directory.GetFiles(directoryFullPath, "*.png",
                    SearchOption.TopDirectoryOnly))
                    File.Delete(filePath);
            else
                // Debug.LogFormat("[XdUnityUI] Create Directory: {0}", EditorUtil.ToUnityPath(directoryPath) + "/" + directoryName);
                AssetDatabase.CreateFolder(EditorUtil.ToAssetPath(directoryPath),
                    Path.GetFileName(directoryFullPath));
        }

        /**
        * SliceSpriteではつかなくなったが､CreateAtlasでは使用する
        */
        private static string ImportSpritePathToOutputPath(string asset)
        {
            var folderName = Path.GetFileName(Path.GetDirectoryName(asset));
            var folderPath = Path.Combine(EditorUtil.GetOutputSpritesFolderAssetPath(), folderName);
            var fileName = Path.GetFileName(asset);
            return Path.Combine(folderPath, fileName);
        }

#if UNITY_2019_1_OR_NEWER
        private static void CreateAtlas(string name, List<string> importPaths)
        {
            var filename = Path.Combine(EditorUtil.GetBaumAtlasAssetPath(), name + ".spriteatlas");

            var atlas = new SpriteAtlas();
            var settings = new SpriteAtlasPackingSettings
            {
                padding = 8,
                enableTightPacking = false
            };
            atlas.SetPackingSettings(settings);
            var textureSettings = new SpriteAtlasTextureSettings
            {
                filterMode = FilterMode.Point,
                generateMipMaps = false,
                sRGB = true
            };
            atlas.SetTextureSettings(textureSettings);

            var textureImporterPlatformSettings = new TextureImporterPlatformSettings {maxTextureSize = 8192};
            atlas.SetPlatformSettings(textureImporterPlatformSettings);

            // iOS用テクスチャ設定
            // ASTCに固定してしまいっている　これらの設定を記述できるようにしたい
            textureImporterPlatformSettings.overridden = true;
            textureImporterPlatformSettings.name = "iPhone";
            textureImporterPlatformSettings.format = TextureImporterFormat.ASTC_4x4;
            atlas.SetPlatformSettings(textureImporterPlatformSettings);

            // アセットの生成
            AssetDatabase.CreateAsset(atlas, EditorUtil.ToAssetPath(filename));

            // ディレクトリを登録する場合
            // var iconsDirectory = AssetDatabase.LoadAssetAtPath<Object>("Assets/ExternalAssets/Baum2/CreatedSprites/UIESMessenger");
            // atlas.Add(new Object[]{iconsDirectory});
        }
#endif

        /// <summary>
        ///     複数階層のフォルダを作成する
        /// </summary>
        /// <param name="path">一番子供のフォルダまでのパスe.g.)Assets/Resources/Sound/</param>
        /// <remarks>パスは"Assets/"で始まっている必要があります。Splitなので最後のスラッシュ(/)は不要です</remarks>
        public static void CreateFolderRecursively(string path)
        {
            path = path.Replace("\\", "/");
            Debug.Assert(path.StartsWith("Assets/"),
                "arg `path` of CreateFolderRecursively doesn't starts with `Assets/`");

            // もう存在すれば処理は不要
            if (AssetDatabase.IsValidFolder(path)) return;

            // スラッシュで終わっていたら除去
            if (path[path.Length - 1] == '/') path = path.Substring(0, path.Length - 1);

            var names = path.Split('/');
            for (var i = 1; i < names.Length; i++)
            {
                var parent = string.Join("/", names.Take(i).ToArray());
                var target = string.Join("/", names.Take(i + 1).ToArray());
                var child = names[i];
                if (!AssetDatabase.IsValidFolder(target)) AssetDatabase.CreateFolder(parent, child);
            }
        }

        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo iLhs, FileInfo iRhs)
            {
                if (iLhs.Name == iRhs.Name) return true;

                return false;
            }

            public int GetHashCode(FileInfo fi)
            {
                var s = fi.Name;
                return s.GetHashCode();
            }
        }
    }
}