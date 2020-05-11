using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OnionRing;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XdUnityUI.Editor
{
    public class TextureUtil
    {
        /// <summary>
        /// 読み込み可能なTextureを作成する
        /// Texture2DをC#ScriptでReadableに変更するには？ - Qiita
        /// https://qiita.com/Katumadeyaruhiko/items/c2b9b4ccdfe51df4ad4a
        /// </summary>
        /// <param name="sourceTexture"></param>
        /// <returns></returns>
        private static Texture2D CreateReadableTexture2D(Texture2D sourceTexture)
        {
            // オプションをRenderTextureReadWrite.sRGBに変更した
            var renderTexture = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(sourceTexture, renderTexture);
            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTexture;
        }

        /// <summary>
        /// バイナリデータを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static byte[] ReadFileToBytes(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var bin = new BinaryReader(fileStream);
            var values = bin.ReadBytes((int) bin.BaseStream.Length);

            bin.Close();

            return values;
        }

        public static Texture2D CreateTextureFromPng(string path)
        {
            var readBinary = ReadFileToBytes(path);

            int pos = 16; // 16バイトから開始

            int width = 0;
            for (int i = 0; i < 4; i++)
            {
                width = width * 256 + readBinary[pos++];
            }

            int height = 0;
            for (int i = 0; i < 4; i++)
            {
                height = height * 256 + readBinary[pos++];
            }

            var texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);

            return texture;
        }


        static Dictionary<string, string> imageHashMap = new Dictionary<string, string>();
        static Dictionary<string, string> imagePathMap = new Dictionary<string, string>();

        public static void ClearImageMap()
        {
            imageHashMap.Clear();
            imagePathMap.Clear();
        }

        public static string GetSameImagePath(string path)
        {
            var fi = new FileInfo(path);
            path = fi.FullName;
            return imagePathMap.ContainsKey(path) ? imagePathMap[path] : path;
        }

        // Textureデータの書き出し
        // 同じファイル名の場合書き込みしない
        private static string CheckWrite(string newPath, byte[] pngData, Hash128 pngHash)
        {
            var fi = new FileInfo(newPath);
            newPath = fi.FullName;

            var hashStr = pngHash.ToString();

            // ハッシュが同じテクスチャがある Shareする
            if (imageHashMap.ContainsKey(hashStr))
            {
                var name = imageHashMap[hashStr];
                // Debug.Log("shared texture " + Path.GetFileName(newPath) + "==" + Path.GetFileName(name));
                imagePathMap[newPath] = name;
                return "Shared other path texture.";
            }

            // ハッシュからのパスを登録
            imageHashMap[hashStr] = newPath;
            // 置き換え対象のパスを登録
            imagePathMap[newPath] = newPath;

            // 同じファイル名のテクスチャがある（前の変換時に生成されたテクスチャ）
            if (File.Exists(newPath))
            {
                var oldPngData = File.ReadAllBytes(newPath);
                // 中身をチェックする
                if (oldPngData.Length == pngData.Length && pngData.SequenceEqual(oldPngData))
                {
                    // 全く同じだった場合、書き込まないでそのまま利用する
                    // UnityのDB更新を防ぐ
                    return "Same texture existed.";
                }
            }

            File.WriteAllBytes(newPath, pngData);
            return "Created new texture.";
        }

        /// <summary>
        /// アセットのイメージをスライスする
        /// 戻り地は、変換リザルトメッセージ
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string SliceSprite(string assetPath)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(assetPath));
            var directoryPath = Path.Combine(EditorUtil.GetOutputSpritesPath(), directoryName);
            var fileName = Path.GetFileName(assetPath);
            // PNGを読み込み、同じサイズのTextureを作成する
            var texture = CreateReadableTexture2D(CreateTextureFromPng(assetPath));
            // LoadAssetAtPathをつかったテクスチャ読み込み サイズが2のべき乗になる　JPGも読める
            //var texture = CreateReadabeTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>(asset));
            if (PreprocessTexture.SlicedTextures == null)
                PreprocessTexture.SlicedTextures = new Dictionary<string, SlicedTexture>();

            var noSlice = fileName.EndsWith("-noslice.png", StringComparison.Ordinal);
            if (noSlice)
            {
                var slicedTexture = new SlicedTexture(texture, new Boarder(0, 0, 0, 0));
                fileName = fileName.Replace("-noslice.png", ".png");
                var newPath = Path.Combine(directoryPath, fileName);
                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = texture.EncodeToPNG();
                var imageHash = texture.imageContentsHash;
                Object.DestroyImmediate(slicedTexture.Texture);
                return CheckWrite(newPath, pngData, imageHash);
            }

            const string pattern = "-9slice,([0-9]+)px,([0-9]+)px,([0-9]+)px,([0-9]+)px\\.png";
            var matches = Regex.Match(fileName, pattern);
            if (matches.Length > 0)
            {
                // 上・右・下・左の端から内側へのオフセット量
                var top = Int32.Parse(matches.Groups[1].Value);
                var right = Int32.Parse(matches.Groups[2].Value);
                var bottom = Int32.Parse(matches.Groups[3].Value);
                var left = Int32.Parse(matches.Groups[4].Value);

                var slicedTexture = new SlicedTexture(texture, new Boarder(left, bottom, right, top));
                fileName = Regex.Replace(fileName, pattern, ".png");
                var newPath = Path.Combine(directoryPath, fileName);

                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = texture.EncodeToPNG();
                var imageHash = texture.imageContentsHash;
                Object.DestroyImmediate(slicedTexture.Texture);
                return CheckWrite(newPath, pngData, imageHash);
            }

            {
                var slicedTexture = TextureSlicer.Slice(texture);
                var newPath = Path.Combine(directoryPath, fileName);

                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = slicedTexture.Texture.EncodeToPNG();
                var imageHash = texture.imageContentsHash;
                Object.DestroyImmediate(slicedTexture.Texture);
                return CheckWrite(newPath, pngData, imageHash);
            }
            // Debug.LogFormat("[XdUnityUI] Slice: {0} -> {1}", EditorUtil.ToUnityPath(asset), EditorUtil.ToUnityPath(newPath));
        }
    }
}