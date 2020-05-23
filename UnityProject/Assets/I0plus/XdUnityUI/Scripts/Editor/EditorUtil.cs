using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// EditorUtil class.
    /// based on Baum2.Editor.EditorUtil class.
    /// </summary>
    public static class EditorUtil
    {
        /// <summary>
        /// 【C#】ドライブ直下からのファイルリスト取得について - Qiita
        ///  https://qiita.com/OneK/items/8b0d02817a9f2a2fbeb0#%E8%A7%A3%E8%AA%AC
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
                    lstBuff.ForEach(delegate(string str)
                    {
                        lstStr.Add(str);
                    });
                }
            }
            catch(UnauthorizedAccessException)
            {
                // アクセスできなかったので無視
            }

            return lstStr;
        }
        
        /// <summary>
        /// Assets以下のパスにする
        /// \を/におきかえる
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToUnityPath(string path)
        {
            path = path.Substring(path.IndexOf("Assets", System.StringComparison.Ordinal));
            if (path.EndsWith("/", System.StringComparison.Ordinal) ||
                path.EndsWith("\\", System.StringComparison.Ordinal)) path = path.Substring(0, path.Length - 1);
            return path.Replace("\\", "/");
        }

        private const string ImportDirectoryMark = "_XdUnityUIImport";

        public static string GetImportDirectoryPath()
        {
            var path = GetPath(ImportDirectoryMark + "1", false);
            if (path != null) return path;
            return GetPath(ImportDirectoryMark);
        }

        /// <summary>
        /// 優先順位に基づき、みつかったマークファイル名を返す
        /// </summary>
        /// <returns></returns>
        public static string GetImportDirectoryMark()
        {
            var marks = new []
            {
                ImportDirectoryMark + "1",
                ImportDirectoryMark
            };
            foreach (var mark in marks)
            {
                var path = GetPath(mark, false);
                if (path != null) return mark;
            }
            return null;
        }


        public static string GetOutputSpritesFolderPath()
        {
            var path = GetPath("_XdUnityUISprites1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUISprites");
        }

        public static string GetOutputPrefabsFolderPath()
        {
            var path = GetPath("_XdUnityUIPrefabs1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIPrefabs");
        }

        public static string GetFontsPath()
        {
            var path = GetPath("_XdUnityUIFonts1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIFonts");
        }

        public static string GetBaumAtlasPath()
        {
            var path = GetPath("_XdUnityUIAtlas1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIAtlas");
        }

        public static string GetPath(string fileName, bool throwException = true)
        {
            var files = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            if (files.Length > 1)
            {
                files = files.Where(x => !x.Contains("Baum2/Sample")).ToArray();
            }

            if (files.Length > 1)
            {
                Debug.LogErrorFormat("{0}ファイルがプロジェクト内に複数個存在します。", fileName);
            }

            if (files.Length == 0)
            {
                if (throwException)
                    throw new System.ApplicationException(string.Format("{0}ファイルがプロジェクト内に存在しません。", fileName));
                return null;
            }

            string path = files[0];
            return path.Substring(0, path.Length - fileName.Length);
        }

        /// <summary>
        /// サブディレクトリを含めたスプライトの出力パスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetBaumSpritesFullPath(string path)
        {
            // サブディレクトリ名を取得する
            var directoryName = Path.GetFileName(Path.GetFileName(path));
            var directoryPath = GetOutputSpritesFolderPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            return directoryFullPath;
        }

        public static Color HexToColor(string hex)
        {
            if (hex[0] == '#')
            {
                hex = hex.Substring(1);
            }

            var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
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