using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniJSON;
using OnionRing;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I0plus.XdUnityUI.Editor
{
    /// <summary>
    /// シリアライズできるDictionaryクラス
    /// </summary>
    public class Dict : Dictionary<string, string>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<string> vals = new List<string>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            vals.Clear();

            var e = GetEnumerator();

            while (e.MoveNext())
            {
                keys.Add(e.Current.Key);
                vals.Add(e.Current.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();

            var cnt = (keys.Count <= vals.Count) ? keys.Count : vals.Count;
            for (var i = 0; i < cnt; ++i)
                this[keys[i]] = vals[i];
        }
    }

    public class TextureUtil
    {
        /// <summary>
        /// Layout.jsonのみ読み込んだときに、過去出力したテクスチャを読み込めるようにするための情報（シェアしていても）
        /// <Hash, path> テクスチャハッシュHashは、pathテクスチャファイルがある、という情報
        /// </summary>
        private static Dictionary<string, string> imageHashMap = new Dict();

        /// <summary>
        /// Layout.jsonのみ読み込んだときに、過去出力したテクスチャを読み込めるようにするための情報（シェアしていても）
        /// <path1, path2> path1のテクスチャは、path2を利用する、という情報
        /// </summary>
        private static Dictionary<string, string> imagePathMap = new Dict();

        /// <summary>
        ///     読み込み可能なTextureを作成する
        ///     Texture2DをC#ScriptでReadableに変更するには？ - Qiita
        ///     https://qiita.com/Katumadeyaruhiko/items/c2b9b4ccdfe51df4ad4a
        /// </summary>
        /// <param name="sourceTexture"></param>
        /// <param name="destY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="destX"></param>
        /// <returns></returns>
        private static Texture2D CreateReadableTexture2D(
            Texture sourceTexture,
            int? destX, int? destY, int? width, int? height)
        {
            // オプションをRenderTextureReadWrite.sRGBに変更した
            var renderTexture = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(sourceTexture, renderTexture);

            // 現在アクティブなレンダーテクスチャを退避
            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            // テクスチャを作成
            var readableTexture = new Texture2D(width ?? sourceTexture.width, height ?? sourceTexture.height);
            // テクスチャをクリア
            var pixels = readableTexture.GetPixels32();
            var clearColor = new Color32(0, 0, 0, 0);
            for (var i = 0; i < pixels.Length; i++) pixels[i] = clearColor;
            readableTexture.SetPixels32(pixels);
            // コピー
            readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height),
                destX ?? 0,
                destY ?? 0);
            readableTexture.Apply();
            // レンダーテクスチャをもとに戻す
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTexture;
        }

        /// <summary>
        ///     バイナリデータを読み込む
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

            var pos = 16; // 16バイトから開始

            var width = 0;
            for (var i = 0; i < 4; i++) width = width * 256 + readBinary[pos++];

            var height = 0;
            for (var i = 0; i < 4; i++) height = height * 256 + readBinary[pos++];

            var texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);

            return texture;
        }

        public static string GetSameImagePath(string path)
        {
            path = path.Replace("\\", "/");
            if (imagePathMap.ContainsKey(path))
            {
                return imagePathMap[path];
            }

            return path;
        }

        public const string ImageHashMapCacheFileName = "ImageHashMap.xdunityui-cache";
        public const string ImagePathMapCacheFileName = "ImagePathMap.xdunityui-cache";

        static void Save(string folderAssetPath)
        {
            var jsonImageHashMap = JsonUtility.ToJson(imageHashMap);
            File.WriteAllText(folderAssetPath + "/" + ImageHashMapCacheFileName, jsonImageHashMap);
            var jsonImagePathMap = JsonUtility.ToJson(imagePathMap);
            File.WriteAllText(folderAssetPath + "/" + ImagePathMapCacheFileName, jsonImagePathMap);
        }

        static void Load(string folderAssetPath)
        {
            try
            {
                var jsonImageHashMap = File.ReadAllText(folderAssetPath + "/" + ImageHashMapCacheFileName);
                imageHashMap = JsonUtility.FromJson<Dict>(jsonImageHashMap);
            }
            catch (Exception ex)
            {
                Debug.Log($"exception: Read ImageHashMap cache. {ex.Message}");
                imageHashMap = new Dict();
            }

            try
            {
                var jsonImagePathMap = File.ReadAllText(folderAssetPath + "/" + ImagePathMapCacheFileName);
                imagePathMap = JsonUtility.FromJson<Dict>(jsonImagePathMap);
            }
            catch (Exception ex)
            {
                Debug.Log($"exception: Read ImagePathMap cache. {ex.Message}");
                imagePathMap = new Dict();
            }
        }

        // Textureデータの書き出し
        // 同じファイル名の場合書き込みしない
        private static string CheckWrite(string writePngPath, byte[] pngData, Hash128 pngHash)
        {
            // HashMapキャッシュファイルを作成のために
            // - \を/にする
            // - 相対パスである
            writePngPath = writePngPath.Replace("\\", "/");
            Load(Path.GetDirectoryName(writePngPath));

            var hashStr = pngHash.ToString();

            // ハッシュが同じテクスチャがある Shareする
            if (imageHashMap.ContainsKey(hashStr))
            {
                var name = imageHashMap[hashStr];
                // Debug.Log("shared texture " + Path.GetFileName(newPath) + "==" + Path.GetFileName(name));
                imagePathMap[writePngPath] = name;
                Save(Path.GetDirectoryName(writePngPath));
                return "Shared other path texture.";
            }

            // ハッシュからのパスを登録
            imageHashMap[hashStr] = writePngPath;
            // 置き換え対象のパスを登録
            imagePathMap[writePngPath] = writePngPath;

            // 同じファイル名のテクスチャがある（前の変換時に生成されたテクスチャ）
            if (File.Exists(writePngPath))
            {
                var oldPngData = File.ReadAllBytes(writePngPath);
                // 中身をチェックする
                if (oldPngData.Length == pngData.Length && pngData.SequenceEqual(oldPngData))
                {
                    // 全く同じだった場合、書き込まないでそのまま利用する
                    // UnityのDB更新を防ぐ
                    Save(Path.GetDirectoryName(writePngPath));
                    return "Same texture existed.";
                }
            }

            File.WriteAllBytes(writePngPath, pngData);
            Save(Path.GetDirectoryName(writePngPath));
            return "Created new texture.";
        }

        /// <summary>
        ///     アセットのイメージをスライスする
        ///     戻り地は、変換リザルトメッセージ
        /// </summary>
        /// <param name="sourceImagePath"></param>
        /// <returns></returns>
        public static string SliceSprite(string sourceImagePath)
        {
            // Debug.Log($"[XdUnityUI] {sourceImagePath}");
            var directoryName = Path.GetFileName(Path.GetDirectoryName(sourceImagePath));
            var outputDirectoryPath = Path.Combine(EditorUtil.GetOutputSpritesFolderAssetPath(), directoryName);
            var sourceImageFileName = Path.GetFileName(sourceImagePath);

            // オプションJSONの読み込み
            Dictionary<string, object> json = null;
            var filePath = Path.Combine(outputDirectoryPath, sourceImageFileName);
            var imageJsonPath = sourceImagePath + ".json";
            if (File.Exists(imageJsonPath))
            {
                var text = File.ReadAllText(imageJsonPath);
                json = Json.Deserialize(text) as Dictionary<string, object>;
            }

            // PNGを読み込み、同じサイズのTextureを作成する
            var sourceTexture = CreateTextureFromPng(sourceImagePath);
            var optionJson = json.GetDic("copy_rect");
            var texture = CreateReadableTexture2D(sourceTexture,
                optionJson?.GetInt("offset_x"),
                optionJson?.GetInt("offset_y"),
                optionJson?.GetInt("width"),
                optionJson?.GetInt("height")
            );

            // LoadAssetAtPathをつかったテクスチャ読み込み サイズが2のべき乗になる　JPGも読める
            // var texture = CreateReadableTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>(asset));
            if (PreprocessTexture.SlicedTextures == null)
                PreprocessTexture.SlicedTextures = new Dictionary<string, SlicedTexture>();

            if (json != null)
            {
                var slice = json.Get("slice");
                switch (slice.ToLower())
                {
                    case "auto":
                        break;
                    case "none":
                    {
                        var slicedTexture = new SlicedTexture(texture, new Boarder(0, 0, 0, 0));
                        var newPath = Path.Combine(outputDirectoryPath, sourceImageFileName);
                        PreprocessTexture.SlicedTextures[sourceImageFileName] = slicedTexture;
                        var pngData = texture.EncodeToPNG();
                        var imageHash = texture.imageContentsHash;
                        Object.DestroyImmediate(slicedTexture.Texture);
                        return CheckWrite(newPath, pngData, imageHash);
                    }
                    case "border":
                    {
                        var border = json.GetDic("slice_border");
                        if (border == null) break; // borderパラメータがなかった

                        // 上・右・下・左の端から内側へのオフセット量
                        var top = border.GetInt("top") ?? 0;
                        var right = border.GetInt("right") ?? 0;
                        var bottom = border.GetInt("bottom") ?? 0;
                        var left = border.GetInt("left") ?? 0;

                        var slicedTexture = new SlicedTexture(texture, new Boarder(left, bottom, right, top));
                        var newPath = Path.Combine(outputDirectoryPath, sourceImageFileName);

                        PreprocessTexture.SlicedTextures[sourceImageFileName] = slicedTexture;
                        var pngData = texture.EncodeToPNG();
                        var imageHash = texture.imageContentsHash;
                        Object.DestroyImmediate(slicedTexture.Texture);
                        return CheckWrite(newPath, pngData, imageHash);
                    }
                }
            }

            {
                // JSONがない場合、slice:auto
                // ToDo:ここはnoneにするべき
                var slicedTexture = TextureSlicer.Slice(texture);
                PreprocessTexture.SlicedTextures[sourceImageFileName] = slicedTexture;
                var pngData = slicedTexture.Texture.EncodeToPNG();
                var imageHash = texture.imageContentsHash;
                Object.DestroyImmediate(slicedTexture.Texture);
                return CheckWrite(filePath, pngData, imageHash);
            }
            // Debug.LogFormat("[XdUnityUI] Slice: {0} -> {1}", EditorUtil.ToUnityPath(asset), EditorUtil.ToUnityPath(newPath));
        }
    }
}