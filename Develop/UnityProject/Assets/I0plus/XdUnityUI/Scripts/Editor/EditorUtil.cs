using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    ///     EditorUtil class.
    ///     based on Baum2.Editor.EditorUtil class.
    /// </summary>
    public static class EditorUtil
    {
        private const string ImportFolderMarkFilename = "_XdUnityUIImport";

        /// <summary>
        ///     【C#】ドライブ直下からのファイルリスト取得について - Qiita
        ///     https://qiita.com/OneK/items/8b0d02817a9f2a2fbeb0#%E8%A7%A3%E8%AA%AC
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static List<string> GetAllFiles(string dirPath)
        {
            var lstStr = new List<string>();

            try
            {
                // ファイル取得
                var strBuff = Directory.GetFiles(dirPath);
                lstStr.AddRange(strBuff);

                // ディレクトリの取得
                strBuff = Directory.GetDirectories(dirPath);
                foreach (var s in strBuff)
                {
                    var lstBuff = GetAllFiles(s);
                    lstBuff.ForEach(delegate(string str) { lstStr.Add(str); });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // アクセスできなかったので無視
            }

            return lstStr;
        }

        /// <summary>
        ///     Assets以下のパスにする
        ///     \を/におきかえる
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToAssetPath(string path)
        {
            path = path.Replace("\\", "/");
            var assetPath = path;
            if (path.StartsWith(Application.dataPath))
                assetPath = "Assets" + path.Substring(Application.dataPath.Length);
            return assetPath;
        }

        public static string FindFileAssetPath(string fileName, bool throwException = true)
        {
            var guids = AssetDatabase.FindAssets(fileName);
            if (guids.Length == 0)
            {
                if (throwException)
                    throw new ApplicationException($"{fileName}ファイルがプロジェクト内に存在しません。");
                return null;
            }

            if (guids.Length > 1) Debug.LogErrorFormat("{0}ファイルがプロジェクト内に複数個存在します。", fileName);

            var fileAssetPath
                = AssetDatabase.GUIDToAssetPath(guids[0]);

            return fileAssetPath;
        }

        public static string FindFolderAssetPath(string fileName, bool throwException = true)
        {
            var fileAssetPath = FindFileAssetPath(fileName, throwException);
            return Path.GetDirectoryName(fileAssetPath)?.Replace("\\","/");
        }

        /// <summary>
        ///     優先順位に基づき、みつかったマークファイル名を返す
        /// </summary>
        /// <returns></returns>
        public static string GetImportFolderAssetPath()
        {
            var markFiles = new[]
            {
                ImportFolderMarkFilename + "1",
                ImportFolderMarkFilename
            };
            foreach (var markFile in markFiles)
            {
                var path = FindFolderAssetPath(markFile, false);
                if (path != null) return path;
            }

            return null;
        }

        public static string GetOutputSpritesFolderAssetPath()
        {
            var path = FindFolderAssetPath("_XdUnityUISprites1", false);
            if (path != null) return path;
            return FindFolderAssetPath("_XdUnityUISprites");
        }

        public static string GetOutputPrefabsFolderAssetPath()
        {
            var path = FindFolderAssetPath("_XdUnityUIPrefabs1", false);
            if (path != null) return path;
            return FindFolderAssetPath("_XdUnityUIPrefabs");
        }

        public static string GetFontsAssetPath()
        {
            var path = FindFolderAssetPath("_XdUnityUIFonts1", false);
            if (path != null) return path;
            return FindFolderAssetPath("_XdUnityUIFonts");
        }

        public static string GetBaumAtlasAssetPath()
        {
            var path = FindFolderAssetPath("_XdUnityUIAtlas1", false);
            if (path != null) return path;
            return FindFolderAssetPath("_XdUnityUIAtlas");
        }

        /// <summary>
        ///     サブディレクトリを含めたスプライトの出力パスを取得する
        /// </summary>
        /// <param name="spritePath"></param>
        /// <returns></returns>
        public static string GetSpriteFolderAssetPath(string spritePath)
        {
            // サブディレクトリ名を取得する
            var directoryName = Path.GetFileName(Path.GetFileName(spritePath));
            var directoryPath = GetOutputSpritesFolderAssetPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            return directoryFullPath;
        }

        public static string GetPrefabFolderAssetPath(string prefabPath)
        {
            // サブディレクトリ名を取得する
            var directoryName = Path.GetFileName(Path.GetFileName(prefabPath));
            var directoryPath = GetOutputPrefabsFolderAssetPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            return directoryFullPath;
        }

        /**
         * /Assets/Top/Second/File.txt
         * return Second
         */
        public static string GetSubFolderName(string filePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);
            return Path.GetFileName(folderPath);
        }

        public static Color HexToColor(string hex)
        {
            if (hex[0] == '#') hex = hex.Substring(1);

            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        public static RectTransform CopyTo(this RectTransform self, RectTransform to)
        {
            to.sizeDelta = self.sizeDelta;
            to.position = self.position;
            return self;
        }

        public static Image CopyTo(this Image self, Image to)
        {
            to.sprite = self.sprite;
            to.type = self.type;
            to.color = self.color;
            return self;
        }
    }
}