using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.U2D;
using UnityEngine.U2D;
#endif
using Object = UnityEngine.Object;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// based on Baum2/Editor/Scripts/BaumImporter file.
    /// </summary>
    public sealed class updateDisplayProgressBar : AssetPostprocessor
    {
        public override int GetPostprocessOrder()
        {
            return 1000;
        }

        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo iLhs, FileInfo iRhs)
            {
                if (iLhs.Name == iRhs.Name)
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(FileInfo fi)
            {
                var s = fi.Name;
                return s.GetHashCode();
            }
        }

        private static int progressTotal = 1;
        private static int progressCount = 0;

        private static void UpdateDisplayProgressBar(string message = "")
        {
            if (progressTotal > 1)
            {
                EditorUtility.DisplayProgressBar("XdUnitUI Import",
                    string.Format("{0}/{1} {2}", progressCount, progressTotal, message),
                    ((float) progressCount / progressTotal));
            }
        }

        private const string FolderLookMenuPath = "Assets/XdUnityUI/Auto Import Enable";
        private static bool _autoEnableFlag = false; // デフォルトがチェック済みの時には true にする

        [MenuItem(FolderLookMenuPath)]
        public static void SampleMenu()
        {
            _autoEnableFlag = !_autoEnableFlag;
            Menu.SetChecked(FolderLookMenuPath, _autoEnableFlag);
        }

        [MenuItem(FolderLookMenuPath, true)]
        public static bool TestMenuValidate()
        {
            Menu.SetChecked(FolderLookMenuPath, _autoEnableFlag);
            return true;
        }

        /// <summary>
        /// 自動インポート
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (_autoEnableFlag)
                Import(importedAssets, movedAssets);
        }

        /// <summary>
        /// メニューから起動し、Importフォルダ内にあるファイルを消去する
        /// </summary>
        [MenuItem("Assets/XdUnityUI/Import(delete import assets)")]
        public static void MenuImportDeleteAssets()
        {
            MenuImport(true);
        }
        
        [MenuItem("Assets/XdUnityUI/Import")]
        public static void MenuImportNoDeleteAssets()
        {
            MenuImport(false);
        }

        public static void MenuImport( bool deleteAssetsFlag)
        {
            var importedAssets = new List<string>();

            var importDirectoryPath = EditorUtil.GetImportDirectoryPath();
            var importDirectoryMark = EditorUtil.GetImportDirectoryMark();

            // ファイルの追加
            var files = Directory.EnumerateFiles(
                importDirectoryPath, "*", System.IO.SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".meta" || Path.GetFileName(file) == importDirectoryMark) continue;
                importedAssets.Add(EditorUtil.ToUnityPath(file));
            }

            // ディレクトリの追加
            var dirs = Directory.EnumerateDirectories(
                importDirectoryPath, "*", SearchOption.AllDirectories);

            foreach (var dir in dirs)
            {
                importedAssets.Add(EditorUtil.ToUnityPath(dir));
            }

            Import(importedAssets, null, deleteAssetsFlag);
        }

        private static bool IsDirectory(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception exception)
            {
            }

            return false;
        }

        private static void DeleteEntry(string path)
        {
            File.Delete(path);
        }


        /// <summary>
        /// Assetディレクトリに追加されたファイルを確認、インポート処理を行う
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="deleteImportEntriesFlag"></param>
        private static void Import(IReadOnlyCollection<string> importedAssets, IReadOnlyCollection<string> movedAssets,
            bool deleteImportEntriesFlag = false)
        {
            var importDirectoryPath = EditorUtil.ToUnityPath(EditorUtil.GetImportDirectoryPath());

            progressTotal = importedAssets.Count;
            if (movedAssets != null) progressCount += movedAssets.Count;
            if (progressTotal == 0) return;
            progressCount = 0;

            var changed = false;

            // スプライト出力フォルダの作成
            foreach (var importedAsset in importedAssets)
            {
                // 入力アセットがインポートフォルダ内あるか
                if (!importedAsset.Contains(importDirectoryPath)) continue;
                if (!IsDirectory(importedAsset)) continue;
                // ディレクトリであった場合
                var exportPath = EditorUtil.GetBaumSpritesFullPath(importedAsset);
                var importPath = Path.GetFullPath(importedAsset);
                if (Directory.Exists(exportPath))
                {
                    // すでにあるフォルダ　インポートファイルと比較して、出力先にある必要のないファイルを削除する
                    // ダブっている分は比較し、異なっている場合に上書きするようにする
                    var exportInfo = new DirectoryInfo(exportPath);
                    var importInfo = new DirectoryInfo(importPath);

                    var list1 = exportInfo.GetFiles("*.png", SearchOption.AllDirectories);
                    var list2 = importInfo.GetFiles("*.png", SearchOption.AllDirectories);

                    // exportフォルダにある importにはないファイルをリストアップする
                    // 注意：
                    // 　-no-slice -9slice付きのファイルなどは、イメージ名が変更されexportフォルダに入るので
                    // 　差分としてでる
                    if (deleteImportEntriesFlag)
                    {
                        var deleteEntries = list1.Except(list2, new FileInfoComparer());

                        foreach (var fileInfo in deleteEntries)
                        {
                            var deleteFileName = fileInfo.FullName;
                            //fileInfo.Delete();
                            DeleteEntry(fileInfo.FullName);
                            //File.Delete(deleteFileName + ".meta");
                            DeleteEntry(deleteFileName + ".meta");
                            changed = true;
                        }
                    }
                }
                else
                {
                    VerticalLayoutGroup a;
                    CreateSpritesDirectory(importedAsset);
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
            EditorApplication.delayCall += () =>
            {
                // SpriteイメージのハッシュMapをクリアしたかどうかのフラグ
                // importedAssetsに一気に全部の新規ファイルが入ってくる前提の処理
                // 全スライス処理が走る前、最初にClearImageMapをする
                var clearedImageMap = false;
                // 画像コンバート　スライス処理
                var messageCounter = new Dictionary<string, int>();
                foreach (var importedAsset in importedAssets)
                {
                    if (!importedAsset.Contains(importDirectoryPath)) continue;
                    if (!importedAsset.EndsWith(".png", System.StringComparison.Ordinal)) continue;
                    //
                    if (!clearedImageMap)
                    {
                        TextureUtil.ClearImageMap();
                        clearedImageMap = true;
                    }

                    // スライス処理
                    var message = TextureUtil.SliceSprite(importedAsset);
                    changed = true;
                    
                    // 元画像を削除する
                    if (deleteImportEntriesFlag)
                    {
                        File.Delete(Path.GetFullPath(importedAsset));
                        File.Delete(Path.GetFullPath(importedAsset) + ".meta");
                        // AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                    }

                    progressCount += 1;
                    UpdateDisplayProgressBar(message);

                    // 出力されたログをカウントする
                    if (messageCounter.ContainsKey(message))
                    {
                        messageCounter[message] = messageCounter[message] + 1;
                    }
                    else
                    {
                        messageCounter.Add(message, 1);
                    }
                }

                foreach (var keyValuePair in messageCounter)
                {
                    Debug.Log($"[XdUnityUI] {keyValuePair.Key}:{keyValuePair.Value}");
                }

                if (changed)
                {
                    AssetDatabase.Refresh();
                    changed = false;
                }

                EditorApplication.delayCall += () =>
                {
                    // import ディレクトリ削除
                    foreach (var asset in importedAssets)
                    {
                        if (!asset.Contains(importDirectoryPath)) continue;
                        if (!IsDirectory(asset)) continue;
                        var fullPath = Path.GetFullPath(asset);
                        // ディレクトリが空っぽかどうか調べる　コンバート用PNGファイルがはいっていた場合、
                        // 変換後削除されるため、すべて変換された場合、空になる
                        if (Directory.EnumerateFileSystemEntries(fullPath).Any()) continue;
                        // 空であれば削除
                        // Debug.LogFormat("[XdUnityUI] Delete Directory: {0}", EditorUtil.ToUnityPath(asset));
                        AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                    }

                    // Create Prefab
                    foreach (var asset in importedAssets)
                    {
                        UpdateDisplayProgressBar("layout");
                        progressCount += 1;
                        if (!asset.Contains(importDirectoryPath)) continue;
                        if (!asset.EndsWith(".layout.json", System.StringComparison.Ordinal)) continue;

                        var name = Path.GetFileName(asset).Replace(".layout.json", "");
                        var spriteRootPath =
                            EditorUtil.ToUnityPath(Path.Combine(EditorUtil.GetOutputSpritesPath(), name));
                        var fontRootPath = EditorUtil.ToUnityPath(EditorUtil.GetFontsPath());
                        var creator = new PrefabCreator(spriteRootPath, fontRootPath, asset);
                        var go = creator.Create();
                        var savePath =
                            EditorUtil.ToUnityPath(Path.Combine(EditorUtil.GetOutputPrefabsPath(), name + ".prefab"));
                        try
                        {
#if UNITY_2018_3_OR_NEWER
                            var savedAsset = PrefabUtility.SaveAsPrefabAsset(go, savePath);
                            Debug.Log("[XdUnityUI] Created prefab: " + savePath, savedAsset);
#else
                            Object originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
                            if (originalPrefab == null) originalPrefab = PrefabUtility.CreateEmptyPrefab(savePath);
                            PrefabUtility.ReplacePrefab(go, originalPrefab, ReplacePrefabOptions.ReplaceNameBased);
#endif
                        }
                        catch
                        {
                            // 変換中例外が起きた場合もテンポラリGameObjectを削除する
                            Object.DestroyImmediate(go);
                            EditorUtility.ClearProgressBar();
                            throw;
                        }

                        // 作成に成功した
                        Object.DestroyImmediate(go);
                        if (deleteImportEntriesFlag)
                        {
                            // layout.jsonを削除する
                            AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                        }
                    }
                    EditorUtility.ClearProgressBar();
                };
            };
        }

        private static void CreateSpritesDirectory(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetFileName(asset));
            var directoryPath = EditorUtil.GetOutputSpritesPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            if (Directory.Exists(directoryFullPath))
            {
                // 画像出力用フォルダに画像がのこっていればすべて削除
                // Debug.LogFormat("[XdUnityUI] Delete Exist Sprites: {0}", EditorUtil.ToUnityPath(directoryFullPath));
                foreach (var filePath in Directory.GetFiles(directoryFullPath, "*.png", SearchOption.TopDirectoryOnly))
                    File.Delete(filePath);
            }
            else
            {
                // Debug.LogFormat("[XdUnityUI] Create Directory: {0}", EditorUtil.ToUnityPath(directoryPath) + "/" + directoryName);
                AssetDatabase.CreateFolder(EditorUtil.ToUnityPath(directoryPath), Path.GetFileName(directoryFullPath));
            }
        }

        /**
        * SliceSpriteではつかなくなったが､CreateAtlasでは使用する
        */
        private static string ImportSpritePathToOutputPath(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(asset));
            var directoryPath = Path.Combine(EditorUtil.GetOutputSpritesPath(), directoryName);
            var fileName = Path.GetFileName(asset);
            return Path.Combine(directoryPath, fileName);
        }

#if UNITY_2019_1_OR_NEWER
        private static void CreateAtlas(string name, List<string> importPaths)
        {
            var filename = Path.Combine(EditorUtil.GetBaumAtlasPath(), name + ".spriteatlas");

            var atlas = new SpriteAtlas();
            var settings = new SpriteAtlasPackingSettings()
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
            AssetDatabase.CreateAsset(atlas, EditorUtil.ToUnityPath(filename));

            // ディレクトリを登録する場合
            // var iconsDirectory = AssetDatabase.LoadAssetAtPath<Object>("Assets/ExternalAssets/Baum2/CreatedSprites/UIESMessenger");
            // atlas.Add(new Object[]{iconsDirectory});
        }
#endif
    }
}