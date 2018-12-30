//------------------------------------------------------------------------------
// EditorUtils.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 2/10/2014
// Owner: Habib Loew
//
// Provides common editor utilities
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CYM.Utile
{

    public class EditorUtils {

        //
        // Useful constants
        //
        public static readonly int MaxTextAreaStringLength = 15000;

        public enum EolType {
            Windows,
            Unix
        }


        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        public static String EolTypeToEolString (EolType eolType) {

            switch (eolType) {
                case EolType.Unix:
                    return "\n";

                case EolType.Windows:
                    return "\r\n";

            }

            throw new Exception(String.Format("Unsupported EOL string!"));

        }

        //------------------------------------------------------------------------------
        public static EolType EolStringToEolType (String eol) {

            switch (eol) {
                case "\r\n":
                    return EolType.Windows;

                case "\n":
                    return EolType.Unix;
            }

            throw new Exception(String.Format("Unsupported EOL string: {0}", Regex.Escape(eol)));

        }

        //------------------------------------------------------------------------------
        public static String NormalizeLineEndings (String text, EolType eol) {

            String eolString = (eol == EolType.Unix) ? "\n" : "\r\n";

            String normalized = Regex.Replace(text, @"\r*\n", eolString, RegexOptions.Multiline);
            return normalized;

        }

        //------------------------------------------------------------------------------
        // Find asset paths by asset type
        public static List<String> FindAssetPaths<T> () where T : UnityEngine.Object {

            String assetFilter = String.Format("t:{0}", typeof(T).ToString());

            String [] assetGuids = AssetDatabase.FindAssets(assetFilter);
            List<String> assetFilePaths = new List<string>();

            foreach (String guidString in assetGuids) {
                assetFilePaths.Add(AssetDatabase.GUIDToAssetPath(guidString));
            }

            return assetFilePaths;

        }

        //------------------------------------------------------------------------------
        // Find asset paths by label - note that substrings match
        public static List<String> FindAssetPaths (String label) {

            String assetFilter = String.Format("l:{0}", label);

            String [] assetGuids = AssetDatabase.FindAssets(assetFilter);
            List<String> assetFilePaths = new List<string>();

            foreach (String guidString in assetGuids) {
                assetFilePaths.Add(AssetDatabase.GUIDToAssetPath(guidString));
            }

            return assetFilePaths;

        }

        //------------------------------------------------------------------------------
        // Returns the path of the first asset which fully matches the specified name.  
        // Returns null otherwise.
        public static String GetAssetPathFromName (String assetName) {

            foreach (String path in AssetDatabase.GetAllAssetPaths()) {
                if (path.EndsWith(assetName)) {
                    return path;
                }
            }

            return null;

        }

        //------------------------------------------------------------------------------
        // Returns the relative path (from the project directory) of the directory holding
        // the currently selected asset, or null if no asset is selected.
        public static String GetDirectoryPathOfSelectedAsset () {

            UnityEngine.Object[] selectedObjects = Selection.GetFiltered(
                typeof(UnityEngine.Object),
                SelectionMode.Assets
            );

            if (selectedObjects.Length == 0)
                return null;

            String path = AssetDatabase.GetAssetPath(selectedObjects[0]);
            if (File.Exists(path)) {
                path = Path.GetDirectoryName(path);
            }

            return path;

        }

        //------------------------------------------------------------------------------
        // Return a path based upon the input path which is guaranteed to contain 
        // requiredFolder.  If needed, requiredFolder is created and appended to 
        // the path.
        public static String ValidateAssetPath (String path, String requiredFolder) {

            if (String.IsNullOrEmpty(path)) {
                path = "Assets";
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(path, String.Format(@"\b{0}\b", requiredFolder))) {
                String newPath = String.Format("{0}/{1}", path, requiredFolder);
                if (!AssetDatabase.IsValidFolder(newPath)) {
                    AssetDatabase.CreateFolder(path, requiredFolder);
                }

                path = newPath;
            }

            return path;

        }

    }
}

