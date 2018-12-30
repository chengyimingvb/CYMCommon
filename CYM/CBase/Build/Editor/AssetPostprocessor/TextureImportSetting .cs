//------------------------------------------------------------------------------
// TextureImportSetting .cs
// Copyright 2018 2018/3/3 
// Created by CYM on 2018/3/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;

namespace CYM
{
    public class TextureImportSetting : AssetPostprocessor
    {
        /// <summary>
        /// 图片导入之前调用，可设置图片的格式、Tag……
        /// </summary>
        void OnPreprocessTexture()
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            //importer.textureType = TextureImporterType.Sprite; // 设置为Sprite类型
            //importer.mipmapEnabled = false; // 禁用mipmap
            //importer.spritePackingTag = "tag"; // 设置Sprite的打包Tag

            importer.crunchedCompression = true;
            importer.compressionQuality = 100;

        }

        /// <summary>
        /// 图片已经被压缩、保存到指定目录下之后调用
        /// </summary>
        /// <param name="texture"></param>
        void OnPostprocessTexure(Texture2D texture)
        {

        }

        /// <summary>
        /// 所有资源被导入、删除、移动完成之后调用
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {

            }
            foreach (string str in deletedAssets)
            {

            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
    
            }
        }
    }
}